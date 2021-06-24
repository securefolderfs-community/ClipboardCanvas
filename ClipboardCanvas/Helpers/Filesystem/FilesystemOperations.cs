using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.SafetyHelpers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ClipboardCanvas.Helpers.Filesystem
{
    public static class FilesystemOperations
    {
        public static async Task<SafeWrapperResult> CopyFileAsync(IStorageFile source, IStorageFile destination, Action<float> progressReportDelegate, CancellationToken cancellationToken)
        {
            long fileSize = await StorageHelpers.GetFileSize(source);
            byte[] buffer = new byte[Constants.FileSystem.COPY_FILE_BUFFER_SIZE];
            SafeWrapperResult result = SafeWrapperResult.S_SUCCESS;

            using (Stream sourceStream = (await source.OpenReadAsync()).AsStreamForRead())
            {
                using (Stream destinationStream = (await destination.OpenAsync(FileAccessMode.ReadWrite)).AsStreamForWrite())
                {
                    long bytesTransferred = 0L;
                    int currentBlockSize = 0;

                    while ((currentBlockSize = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        bytesTransferred += currentBlockSize;
                        float percentage = (float)bytesTransferred * 100.0f / (float)fileSize;

                        await destinationStream.WriteAsync(buffer, 0, currentBlockSize);
                        progressReportDelegate?.Invoke(percentage);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            // TODO: Delete copied file there
                            result = SafeWrapperResult.S_CANCEL;
                            break;
                        }
                    }
                }
            }

            return result;
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
    }
}
