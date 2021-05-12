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
            long fileSize = await StorageItemHelpers.GetFileSize(source);
            byte[] buffer = new byte[Constants.FileSystem.COPY_FILE_BUFFER_SIZE];
            SafeWrapperResult result = SafeWrapperResult.S_SUCCESS;

            using (IRandomAccessStreamWithContentType sourceStream = await source.OpenReadAsync())
            {
                using (IRandomAccessStream destinationStream = await destination.OpenAsync(FileAccessMode.ReadWrite))
                {
                    long totalBytes = 0L;
                    int currentBlockSize = 0;

                    while ((currentBlockSize = await sourceStream.AsStream().ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        totalBytes += currentBlockSize;
                        float percentage = (float)totalBytes * 100.0f / (float)fileSize;

                        await destinationStream.AsStreamForWrite().WriteAsync(buffer, 0, currentBlockSize);
                        progressReportDelegate?.Invoke(percentage);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            // TODO: Delete copied file there
                            result = new SafeWrapperResult(OperationErrorCode.InProgress, new Exception(), "The operation was canceled");
                            break;
                        }
                    }
                }
            }

            return result;
        }

        public static async Task<SafeWrapperResult> RenameItemAsync(IStorageItem item, string newName, NameCollisionOption collision = NameCollisionOption.GenerateUniqueName)
        {
            SafeWrapperResult result = await SafeWrapperRoutines.SafeWrapAsync(async () => await item.RenameAsync(newName, collision).AsTask());
            return result;
        }
    }
}
