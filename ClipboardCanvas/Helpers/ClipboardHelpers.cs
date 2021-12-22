using Windows.ApplicationModel.DataTransfer;

using ClipboardCanvas.Helpers.SafetyHelpers;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Linq;

namespace ClipboardCanvas.Helpers
{
    public static class ClipboardHelpers
    {
        private static readonly object clipboardLock = new object();

        public static void CopyDataPackage(DataPackage data)
        {
            if (data != null)
            {
                lock (clipboardLock)
                {
                    data.RequestedOperation = DataPackageOperation.Copy;
                    Clipboard.SetContent(data);
                    Clipboard.Flush();
                }
            }
        }

        public static SafeWrapper<DataPackageView> GetClipboardData()
        {
            return SafeWrapperRoutines.SafeWrap(Clipboard.GetContent);
        }

        public static async Task<SafeWrapper<string>> SafeGetTextAsync(this DataPackageView dataPackage)
        {
            // TODO: Add dataPackage.RequestAccessAsync()
            return await SafeWrapperRoutines.SafeWrapAsync(() => dataPackage.GetTextAsync().AsTask());
        }

        public static async Task<SafeWrapper<IReadOnlyList<IStorageItem>>> SafeGetStorageItemsAsync(this DataPackageView dataPackage)
        {
            return await SafeWrapperRoutines.SafeWrapAsync(() => dataPackage.GetStorageItemsAsync().AsTask());
        }

        public static async Task<SafeWrapper<RandomAccessStreamReference>> SafeGetBitmapAsync(this DataPackageView dataPackage)
        {
            return await SafeWrapperRoutines.SafeWrapAsync(() => dataPackage.GetBitmapAsync().AsTask());
        }

        public static async Task<SafeWrapper<Uri>> SafeGetUriAsync(this DataPackageView dataPackage)
        {
            return await SafeWrapperRoutines.SafeWrapAsync(() => dataPackage.GetUriAsync().AsTask());
        }
    }
}
