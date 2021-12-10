using System;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

using ClipboardCanvas.Enums;
using ClipboardCanvas.Exceptions;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.UnsafeNative;
using Vanara.PInvoke;
using System.Runtime.InteropServices;
using System.Text;

namespace ClipboardCanvas.ReferenceItems
{
    /// <summary>
    /// A file containing data and reference to an actual file
    /// </summary>
    public sealed class ReferenceFile
    {
        private readonly StorageFile _innerReferenceFile;

        private ReferenceFileData _referenceFileData;

        public SafeWrapperResult LastError { get; private set; } = SafeWrapperResult.SUCCESS;

        public IStorageItem ReferencedItem { get; private set; }

        private ReferenceFile(StorageFile innerFile, IStorageItem referencedItem)
        {
            this._innerReferenceFile = innerFile;
            this.ReferencedItem = referencedItem;
        }

        /// <summary>
        /// Updates the underlying file and fileData
        /// </summary>
        /// <param name="newPath"></param>
        /// <returns></returns>
        private async Task<SafeWrapperResult> UpdateReference(string newPath, bool regenerateReferenceItem)
        {
            if (!StorageHelpers.Existsh(newPath))
            {
                return new SafeWrapperResult(OperationErrorCode.NotFound, new FileNotFoundException(), "File does not exist.");
            }

            long fileId = GetFileId(newPath);
            bool isRepairable = fileId != -1;

            _referenceFileData = new ReferenceFileData(newPath, fileId, isRepairable);
            string serialized = JsonConvert.SerializeObject(_referenceFileData, Formatting.Indented);

            SafeWrapperResult writeFileTextResult = await FilesystemOperations.WriteFileText(_innerReferenceFile, serialized);
            if (!writeFileTextResult)
            {
                return writeFileTextResult;
            }

            if (regenerateReferenceItem)
            {
                SafeWrapper<IStorageItem> referencedItem = await StorageHelpers.ToStorageItemWithError<IStorageItem>(newPath);
                this.ReferencedItem = referencedItem.Result;

                return referencedItem;
            }

            return SafeWrapperResult.SUCCESS;
        }

        public static async Task<ReferenceFile> CreateReferenceFileFromFile(StorageFile emptyReferenceFile, IStorageItem referencedItem)
        {
            if (!FileHelpers.IsPathEqualExtension(emptyReferenceFile.Path, Constants.FileSystem.REFERENCE_FILE_EXTENSION))
            {
                return new ReferenceFile(emptyReferenceFile, null)
                {
                    LastError = new SafeWrapperResult(OperationErrorCode.InvalidArgument, new ArgumentException(), "Empty file uses invalid Reference File extension.")
                };
            }

            ReferenceFile referenceFile = new ReferenceFile(emptyReferenceFile, referencedItem);

            SafeWrapperResult result = await referenceFile.UpdateReference(referencedItem.Path, false);
            referenceFile.LastError = result;

            return referenceFile;
        }

        private static async Task<SafeWrapper<ReferenceFileData>> ReadData(StorageFile referenceFile)
        {
            SafeWrapper<string> data = await FilesystemOperations.ReadFileText(referenceFile);
            if (!data)
            {
                return (null, data.Details);
            }

            ReferenceFileData referenceFileData = JsonConvert.DeserializeObject<ReferenceFileData>(data);
            return (referenceFileData, SafeWrapperResult.SUCCESS);
        }

        public static async Task<ReferenceFile> GetReferenceFile(StorageFile file)
        {
            // The file is not a Reference File
            if (!IsReferenceFile(file))
            {
                return null;
            }

            // The referenceFile does not exist
            if (!StorageHelpers.Existsh(file.Path))
            {
                return new ReferenceFile(file, null)
                {
                    LastError = new SafeWrapperResult(OperationErrorCode.NotFound, new FileNotFoundException(), "Couldn't resolve item associated with path.")
                };
            }

            SafeWrapper<ReferenceFileData> referenceFileData = await ReadData(file);

            ReferenceFile referenceFile = await GetFile(file, referenceFileData);
            if (referenceFile.LastError)
            {
                if (!referenceFileData.Result.isRepairable) // Bad FileId but the path is correct
                {
                    // Repair the FileId
                    await referenceFile.UpdateReference(referenceFileData.Result.path, false);
                }
            }
            else
            {
                if (referenceFile.LastError == OperationErrorCode.InvalidArgument || referenceFile.LastError == OperationErrorCode.NotFound || referenceFile.LastError == OperationErrorCode.UnknownFailed)
                {
                    if (referenceFileData.Result?.isRepairable ?? false)
                    {
                        // Repair it
                        SafeWrapper<ReferenceFileData> result = RepairReferenceFile(referenceFile, referenceFileData);
                        referenceFile.LastError = result;
                        if (result)
                        {
                            await referenceFile.UpdateReference(result.Result.path, false);
                            return await GetFile(file, result);
                        }
                    }
                }
            }

            return referenceFile;
        }

        private static async Task<ReferenceFile> GetFile(StorageFile referenceFile, SafeWrapper<ReferenceFileData> referenceFileData)
        {
            if (!referenceFileData)
            {
                return new ReferenceFile(referenceFile, null)
                {
                    LastError = referenceFileData
                };
            }
            else if (string.IsNullOrEmpty(referenceFileData?.Result?.path))
            {
                return new ReferenceFile(referenceFile, null)
                {
                    LastError = new SafeWrapperResult(OperationErrorCode.InvalidArgument, new NullReferenceException(), "Reference File path is null.")
                };
            }

            SafeWrapper<IStorageItem> file = await StorageHelpers.ToStorageItemWithError<IStorageItem>(referenceFileData.Result.path);

            if (!file)
            {
                if (file == OperationErrorCode.NotFound)
                {
                    // If NotFound, use custom exception for LoadCanvasFromCollection()
                    return new ReferenceFile(referenceFile, null)
                    {
                        LastError = new SafeWrapperResult(OperationErrorCode.NotFound, new ReferencedFileNotFoundException(), "The item referenced could not be found.")
                    };
                }
                else
                {
                    return new ReferenceFile(referenceFile, null)
                    {
                        LastError = file
                    };
                }
            }

            return new ReferenceFile(referenceFile, file.Result)
            {
                _referenceFileData = referenceFileData.Result
            };
        }

        private static SafeWrapper<ReferenceFileData> RepairReferenceFile(ReferenceFile referenceFile, ReferenceFileData referenceFileData)
        {
            if (referenceFileData.fileId == -1)
            {
                return (null, referenceFile.LastError);
            }

            // Get path from FileId...
            SafeWrapperResult result = new SafeWrapperResult(OperationErrorCode.NotFound, "The reference file could not be repaired.");

            string volumeRoot = Path.GetPathRoot(referenceFileData.path);
            using var volumeHandle = Kernel32.CreateFile(volumeRoot, Kernel32.FileAccess.GENERIC_READ, FileShare.Read, null, FileMode.Open, FileFlagsAndAttributes.FILE_FLAG_BACKUP_SEMANTICS);
            if (volumeHandle.IsInvalid)
            {
                return (null, result);
            }

            var fileId = new Kernel32.FILE_ID_DESCRIPTOR() { Type = 0, Id = new Kernel32.FILE_ID_DESCRIPTOR.DUMMYUNIONNAME() { FileId = referenceFileData.fileId } };
            fileId.dwSize = (uint)Marshal.SizeOf(fileId);

            using var hFile = Kernel32.OpenFileById(volumeHandle, fileId, Kernel32.FileAccess.GENERIC_READ, FileShare.Read, null, FileFlagsAndAttributes.FILE_FLAG_BACKUP_SEMANTICS);
            if (hFile.IsInvalid)
            {
                return (null, result);
            }

            var stringBuilder = new StringBuilder(4096);
            uint res = Kernel32.GetFinalPathNameByHandle(hFile, stringBuilder, 4095, 0);
            if (res != 0)
            {
                return new SafeWrapper<ReferenceFileData>(new ReferenceFileData(Path.GetFullPath(stringBuilder.ToString().Replace(@"\\?\", "", StringComparison.Ordinal)), referenceFileData.fileId, true), OperationErrorCode.Success);
            }
            else
            {
                return (null, result);
            }
        }

        private static long GetFileId(string path)
        {
            using var hFile = Kernel32.CreateFile(path, Kernel32.FileAccess.GENERIC_READ, FileShare.ReadWrite, null, FileMode.Open, FileFlagsAndAttributes.FILE_FLAG_BACKUP_SEMANTICS);
            if (hFile.IsInvalid)
            {
                return -1L;
            }

            SafeWrapper<long> result = SafeWrapperRoutines.SafeWrap(() =>
            {
                var fileId = Kernel32.GetFileInformationByHandleEx<Kernel32.FILE_ID_INFO>(hFile, Kernel32.FILE_INFO_BY_HANDLE_CLASS.FileIdInfo);
                return BitConverter.ToInt64(fileId.FileId.Identifier, 0);
            });

            return result ? result : -1L;
        }

        public static bool IsReferenceFile(StorageFile file)
        {
            return FileHelpers.IsPathEqualExtension(file?.Path, Constants.FileSystem.REFERENCE_FILE_EXTENSION);
        }
    }
}
