using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Windows.Storage;

using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Contexts.Operations;
using ClipboardCanvas.UnsafeNative;

namespace ClipboardCanvas.Helpers.Filesystem
{
    public static class FilesystemOperations
    {
        public static async Task<SafeWrapperResult> CopyFileAsync(IStorageFile source, IStorageFile destination, IOperationContext operationContext)
        {
            long fileSize = await StorageHelpers.GetFileSize(source);
            byte[] buffer = new byte[Constants.FileSystem.COPY_FILE_BUFFER_SIZE];

            SafeWrapperResult result = SafeWrapperResult.SUCCESS;

            operationContext?.StartOperation();

            SafeFileHandle hFileSource = source.CreateSafeFileHandle(FileAccess.Read);
            SafeFileHandle hFileDestination = destination.CreateSafeFileHandle(FileAccess.ReadWrite);

            if (hFileSource.IsInvalid || hFileDestination.IsInvalid)
            {
                return SafeWrapperResult.UNKNOWN_FAIL;
            }

            using (Stream sourceStream = new FileStream(hFileSource, FileAccess.Read))
            {
                using (Stream destinationStream = new FileStream(hFileDestination, FileAccess.ReadWrite))
                {
                    long bytesTransferred = 0L;
                    int currentBlockSize = 0;

                    while ((currentBlockSize = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        bytesTransferred += currentBlockSize;
                        float percentage = (float)bytesTransferred * 100.0f / (float)fileSize;

                        await destinationStream.WriteAsync(buffer, 0, currentBlockSize);
                        operationContext?.OperationProgress?.Report(percentage);

                        if (operationContext?.CancellationToken.IsCancellationRequested ?? false)
                        {
                            result = SafeWrapperResult.CANCEL;

                            // Delete copied file if it was canceled
                            await DeleteItem(destination, true);

                            break;
                        }
                    }
                }
            }

            operationContext?.FinishOperation(result);

            return result;
        }

        public static async Task<SafeWrapper<string>> ReadFileText(IStorageFile file)
        {
            if (file == null)
            {
                return (null, new SafeWrapperResult(OperationErrorCode.InvalidArgument, new NullReferenceException(), "Provided file is null."));
            }

            SafeWrapper<string> result = await SafeWrapperRoutines.SafeWrapAsync(() => FileIO.ReadTextAsync(file).AsTask());

            return result;
        }

        public static async Task<SafeWrapperResult> WriteFileText(IStorageFile file, string text)
        {
            if (file == null)
            {
                return new SafeWrapperResult(OperationErrorCode.InvalidArgument, new NullReferenceException(), "Provided file is null.");
            }

            SafeWrapperResult result = await SafeWrapperRoutines.SafeWrapAsync(() => FileIO.WriteTextAsync(file, text).AsTask());

            return result;
        }

        public static string ReadFileTexth(string path)
        {
            return UnsafeNativeHelpers.ReadStringFromFile(path);
        }

        public static bool WriteFileTexth(string path, string text)
        {
            return UnsafeNativeHelpers.WriteStringToFile(path, text);
        }

        public static async Task<SafeWrapperResult> RenameItem(IStorageItem item, string newName, NameCollisionOption collision = NameCollisionOption.GenerateUniqueName)
        {
            SafeWrapperResult result = await SafeWrapperRoutines.SafeWrapAsync(async () => await item.RenameAsync(newName, collision).AsTask());
            return result;
        }

        public static async Task<SafeWrapperResult> DeleteItem(IStorageItem item, bool permanently = false)
        {
            if (item == null)
            {
                return new SafeWrapperResult(OperationErrorCode.InvalidArgument, new ArgumentNullException(), "The provided storage item is null.");
            }

            SafeWrapperResult result = await SafeWrapperRoutines.SafeWrapAsync(
               () => item.DeleteAsync(permanently ? StorageDeleteOption.PermanentDelete : StorageDeleteOption.Default).AsTask());

            return result;
        }

        public static async Task<SafeWrapper<StorageFile>> CreateFile(string path, CreationCollisionOption collision = CreationCollisionOption.GenerateUniqueName)
        {
            string parentFolderPath = Path.GetDirectoryName(path);
            SafeWrapper<StorageFolder> parentFolder = await StorageHelpers.ToStorageItemWithError<StorageFolder>(parentFolderPath);

            if (!parentFolder)
            {
                return new SafeWrapper<StorageFile>(null, parentFolder.Details);
            }

            string fileName = RemoveIllegalFileNameCharacters(Path.GetFileName(path));

            return await CreateFile(parentFolder, fileName, collision);
        }

        public static async Task<SafeWrapper<StorageFile>> CreateFile(StorageFolder parentFolder, string fileName, CreationCollisionOption collision = CreationCollisionOption.GenerateUniqueName)
        {
            if (parentFolder == null)
            {
                return new SafeWrapper<StorageFile>(null, OperationErrorCode.InvalidArgument, new ArgumentNullException(), "The provided folder is null.");
            }

            return await SafeWrapperRoutines.SafeWrapAsync(() => parentFolder.CreateFileAsync(RemoveIllegalFileNameCharacters(fileName), collision).AsTask());
        }

        public static async Task<SafeWrapper<StorageFolder>> CreateFolder(string path, CreationCollisionOption collision = CreationCollisionOption.GenerateUniqueName)
        {
            string parentFolderPath = Path.GetDirectoryName(path);
            SafeWrapper<StorageFolder> parentFolder = await StorageHelpers.ToStorageItemWithError<StorageFolder>(parentFolderPath);

            if (!parentFolder)
            {
                return parentFolder;
            }

            string folderName = Path.GetFileName(path);

            return await CreateFolder(parentFolder, folderName, collision);
        }

        public static async Task<SafeWrapper<StorageFolder>> CreateFolder(StorageFolder parentFolder, string name, CreationCollisionOption collision = CreationCollisionOption.GenerateUniqueName)
        {
            if (parentFolder == null)
            {
                return new SafeWrapper<StorageFolder>(null, OperationErrorCode.InvalidArgument, new ArgumentNullException(), "The provided folder is null.");
            }

            return await SafeWrapperRoutines.SafeWrapAsync(() => parentFolder.CreateFolderAsync(name, collision).AsTask());
        }

        private static string RemoveIllegalFileNameCharacters(string dangerousFileName)
        {
            return string.Join("_", dangerousFileName.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
