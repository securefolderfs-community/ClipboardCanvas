using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.UI.Xaml.Media.Imaging;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.Contexts.Operations;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.Filesystem;
using System.IO;

namespace ClipboardCanvas.CanavsPasteModels
{
    public class ImagePasteModel : BasePasteModel
    {
        private bool _wasPastedAsReference;

        public SoftwareBitmap SoftwareBitmap { get; private set; }

        public BitmapImage GifBitmapImage;

        public ImagePasteModel(ICanvasItemReceiverModel canvasFileReceiver, IOperationContextReceiver operationContextReceiver)
            : base (canvasFileReceiver, operationContextReceiver)
        {
        }

        public override Task<SafeWrapper<CanvasItem>> PasteData(DataPackageView dataPackage, bool pasteAsReference, CancellationToken cancellationToken)
        {
            _wasPastedAsReference = pasteAsReference;
            return base.PasteData(dataPackage, pasteAsReference, cancellationToken);
        }

        protected override async Task<SafeWrapperResult> SetDataFromDataPackage(DataPackageView dataPackage)
        {
            SafeWrapper<IRandomAccessStreamWithContentType> openedStream = null;

            if (dataPackage.Contains(StandardDataFormats.Text)) // Url to file
            {
                if (dataPackage.Contains(StandardDataFormats.StorageItems)) // Url to StorageFile
                {
                    SafeWrapper<IReadOnlyList<IStorageItem>> items = await dataPackage.SafeGetStorageItemsAsync();

                    if (!items)
                    {
                        return items;
                    }

                    StorageFile imageFile = items.Result.First() as StorageFile;
                    customName = imageFile.Name;

                    openedStream = await SafeWrapperRoutines.SafeWrapAsync(
                        () => imageFile.OpenReadAsync().AsTask());
                }
                else // Download from url
                {
                    SafeWrapper<string> url = await dataPackage.SafeGetTextAsync();

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
                if (dataPackage.Contains(StandardDataFormats.StorageItems)) // File to image
                {
                    SafeWrapper<IReadOnlyList<IStorageItem>> items = await dataPackage.SafeGetStorageItemsAsync();

                    if (!items)
                    {
                        return items;
                    }

                    IsContentAsReference = _wasPastedAsReference;

                    this.pastedItem = items.Result.First();
                    return await SetDataFromExistingItem(pastedItem);
                }
                else // Just image
                {
                    SafeWrapper<RandomAccessStreamReference> bitmap = await dataPackage.SafeGetBitmapAsync();
                    SafeWrapper<Uri> uri = await dataPackage.SafeGetUriAsync();

                    if (uri)
                    {
                        customName = Path.GetFileName(uri.Result.LocalPath);
                    }

                    if (!bitmap)
                    {
                        return bitmap;
                    }

                    openedStream = await SafeWrapperRoutines.SafeWrapAsync(
                        () => bitmap.Result.OpenReadAsync().AsTask());
                }
            }

            if (!openedStream)
            {
                openedStream.Result?.Dispose();
                return openedStream;
            }

            return await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                using (openedStream.Result)
                {
                    BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(openedStream.Result);
                    SoftwareBitmap = await bitmapDecoder.GetSoftwareBitmapAsync();
                }
            });
        }

        protected override async Task<SafeWrapper<CanvasItem>> GetCanvasFileFromExtension(ICanvasItemReceiverModel canvasFileReceiver)
        {
            return await canvasFileReceiver.CreateNewCanvasItemFromExtension(".png");
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

            return await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                using (openedStream.Result)
                {
                    if (FileHelpers.IsPathEqualExtension(item.Path, ".gif"))
                    {
                        GifBitmapImage = new BitmapImage();
                        await GifBitmapImage.SetSourceAsync(openedStream.Result);
                    }
                    else
                    {
                        BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(openedStream.Result);
                        SoftwareBitmap = await bitmapDecoder.GetSoftwareBitmapAsync();
                    }
                }
            });
        }

        public override void Dispose()
        {
            base.Dispose();

            SoftwareBitmap?.Dispose();
            SoftwareBitmap = null;
        }
    }
}
