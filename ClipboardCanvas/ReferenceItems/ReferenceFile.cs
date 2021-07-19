using ClipboardCanvas.Enums;
using ClipboardCanvas.Exceptions;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.ReferenceItems
{
    /// <summary>
    /// A file containing data and reference to an actual file
    /// </summary>
    public sealed class ReferenceFile
    {
        private readonly StorageFile _innerReferenceFile;

        public SafeWrapperResult LastError { get; private set; } = SafeWrapperResult.S_SUCCESS;

        public IStorageItem ReferencedItem { get; private set; }

        public ReferenceFileData ReferenceFileData { get; private set; }

        private ReferenceFile(StorageFile innerFile, IStorageItem referencedItem)
        {
            this._innerReferenceFile = innerFile;
            this.ReferencedItem = referencedItem;
        }

        public async Task<SafeWrapperResult> UpdateReferenceFile(ReferenceFileData referenceFileData)
        {
            string serialized = JsonConvert.SerializeObject(referenceFileData, Formatting.Indented);
            SafeWrapperResult result = await FilesystemOperations.WriteFileText(_innerReferenceFile, serialized);

            return result;
        }

        internal static async Task<SafeWrapper<ReferenceFileData>> ReadData(StorageFile referenceFile)
        {
            SafeWrapper<string> data = await FilesystemOperations.ReadFileText(referenceFile);

            if (!data)
            {
                return new SafeWrapper<ReferenceFileData>(null, data.Details);
            }

            ReferenceFileData referenceFileData = JsonConvert.DeserializeObject<ReferenceFileData>(data);

            return new SafeWrapper<ReferenceFileData>(referenceFileData, SafeWrapperResult.S_SUCCESS);
        }

        public static async Task<ReferenceFile> GetFile(StorageFile referenceFile)
        {
            // The file is not a Reference File
            if (!IsReferenceFile(referenceFile))
            {
                return null;
            }
            // The file does not exist
            if (!StorageHelpers.Exists(referenceFile.Path))
            {
                return new ReferenceFile(referenceFile, null)
                {
                    LastError = new SafeWrapperResult(OperationErrorCode.NotFound, new FileNotFoundException(), "Couldn't resolve item associated with path.")
                };
            }

            SafeWrapper<ReferenceFileData> referenceFileData = await ReadData(referenceFile);

            return await GetFile(referenceFile, referenceFileData);
        }

        public static async Task<ReferenceFile> GetFile(StorageFile referenceFile, SafeWrapper<ReferenceFileData> referenceFileData)
        {
            if (!referenceFileData || string.IsNullOrEmpty(referenceFileData.Result?.path))
            {
                return new ReferenceFile(referenceFile, null)
                {
                    LastError = referenceFileData
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
                        LastError = (SafeWrapperResult)file
                    };
                }
            }

            return new ReferenceFile(referenceFile, file.Result);
        }

        public static bool IsReferenceFile(StorageFile file)
        {
            return file?.Path.EndsWith(Constants.FileSystem.REFERENCE_FILE_EXTENSION) ?? false;
        }
    }
}
