using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Collections.Generic;
using System.Linq;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.Contexts.Operations;

namespace ClipboardCanvas.CanavsPasteModels
{
    public class ImagePasteModel : BasePasteModel
    {
        public SoftwareBitmap SoftwareBitmap { get; private set; }

        public ImagePasteModel(ICanvasItemReceiverModel canvasFileReceiver, IOperationContextReceiver operationContextReceiver)
            : base (canvasFileReceiver, operationContextReceiver)
        {
        }

        protected override async Task<SafeWrapperResult> SetDataFromDataPackage(DataPackageView dataPackage)
        {
            SafeWrapper<IRandomAccessStreamWithContentType> openedStream = null;

            if (dataPackage.Contains(StandardDataFormats.Text)) // Url to file
            {
                if (dataPackage.Contains(StandardDataFormats.StorageItems)) // Url to StorageFile
                {
                    SafeWrapper<IReadOnlyList<IStorageItem>> items = await SafeWrapperRoutines.SafeWrapAsync(
                        () => dataPackage.GetStorageItemsAsync().AsTask());

                    if (!items)
                    {
                        return items;
                    }

                    StorageFile imageFile = items.Result.First().As<StorageFile>();

                    openedStream = await SafeWrapperRoutines.SafeWrapAsync(
                        () => imageFile.OpenReadAsync().AsTask());
                }
                else // Download from url
                {
                    SafeWrapper<string> url = await SafeWrapperRoutines.SafeWrapAsync(
                        () => dataPackage.GetTextAsync().AsTask());

                    if (!url)
                    {
                        return url;
                    }

                    openedStream = await SafeWrapperRoutines.SafeWrapAsync(
                        async () => await (RandomAccessStreamReference.CreateFromUri(new Uri(url))).OpenReadAsync());
                }
            }
            else if (dataPackage.Contains(StandardDataFormats.Bitmap))
            {
                SafeWrapper<RandomAccessStreamReference> bitmap = await SafeWrapperRoutines.SafeWrapAsync(
                           () => dataPackage.GetBitmapAsync().AsTask());

                if (!bitmap)
                {
                    return bitmap;
                }

                openedStream = await SafeWrapperRoutines.SafeWrapAsync(
                    () => bitmap.Result.OpenReadAsync().AsTask());
            }

            if (!openedStream)
            {
                openedStream.Result?.Dispose();
                return openedStream;
            }

            SafeWrapperResult result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(openedStream.Result);
                SoftwareBitmap = await bitmapDecoder.GetSoftwareBitmapAsync();
            });

            openedStream.Result?.Dispose();
            
            return result;
        }

        protected override async Task<SafeWrapper<CanvasItem>> GetCanvasFileFromExtension(ICanvasItemReceiverModel canvasFileReceiver)
        {
            return await canvasFileReceiver.CreateNewCanvasFileFromExtension(".png");
        }

        protected override async Task<SafeWrapperResult> SaveDataToFile()
        {
            SafeWrapperResult result;

            BitmapEncoder bitmapEncoder = null;
            result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                using (IRandomAccessStream fileStream = await (await sourceFile).OpenAsync(FileAccessMode.ReadWrite))
                {
                    bitmapEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);
                    bitmapEncoder.SetSoftwareBitmap(SoftwareBitmap);
                    await bitmapEncoder.FlushAsync();
                }
            });

            if (!result)
            {
                const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);
                int hresult = result.Exception.HResult;

                result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
                {
                    if (hresult == WINCODEC_ERR_UNSUPPORTEDOPERATION)
                    {
                        using (IRandomAccessStream fileStream = await (await sourceFile).OpenAsync(FileAccessMode.ReadWrite))
                        {
                            bitmapEncoder.IsThumbnailGenerated = false;
                            await bitmapEncoder.FlushAsync();
                        }
                    }
                });
            }

            return result;
        }

        public override async Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
        {
            if (item is not StorageFile file)
            {
                return ItemIsNotAFileResult;
            }

            SafeWrapper<IRandomAccessStream> openedStream = await SafeWrapperRoutines.SafeWrapAsync(
                    () => file.OpenAsync(FileAccessMode.Read).AsTask());

            if (!openedStream)
            {
                return openedStream;
            }

            SafeWrapperResult result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(openedStream.Result);
                SoftwareBitmap = await bitmapDecoder.GetSoftwareBitmapAsync();
            });

            openedStream.Result.Dispose();

            return result;
        }

        public override void Dispose()
        {
            base.Dispose();

            SoftwareBitmap?.Dispose();
            SoftwareBitmap = null;
        }
    }
}
