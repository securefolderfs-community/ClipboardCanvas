using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.IO;
using Windows.Storage.Streams;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

using ClipboardCanvas.Extensions;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Enums;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.DataModels.PastedContentDataModels;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public sealed class ImageCanvasViewModel : BaseCanvasViewModel
    {
        #region Private Members

        private readonly IDynamicCanvasControlView _view;

        private Stream _dataStream;

        private SoftwareBitmap _softwareBitmap;

        #endregion

        #region Protected Properties

        protected override ICollectionModel AssociatedCollection => _view?.CollectionModel;

        #endregion

        #region Public Properties

        public static List<string> Extensions => new List<string>() {
            ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff", ".ico", ".svg", ".webp"
        };

        private BitmapImage _ContentImage;
        public BitmapImage ContentImage
        {
            get => _ContentImage;
            set => SetProperty(ref _ContentImage, value);
        }

        #endregion

        #region Constructor

        public ImageCanvasViewModel(IDynamicCanvasControlView view)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, new ImageContentType())
        {
            this._view = view;
        }

        #endregion

        #region Override

        protected override async Task<SafeWrapperResult> SetData(DataPackageView dataPackage)
        {
            if (dataPackage.Contains(StandardDataFormats.Bitmap))
            {
                SafeWrapper<RandomAccessStreamReference> bitmap = await SafeWrapperRoutines.SafeWrapAsync(
                           () => dataPackage.GetBitmapAsync().AsTask());

                if (!bitmap)
                {
                    Debugger.Break();
                    return (SafeWrapperResult)bitmap;
                }

                SafeWrapper<IRandomAccessStreamWithContentType> openedStream = await SafeWrapperRoutines.SafeWrapAsync(
                    () => bitmap.Result.OpenReadAsync().AsTask());

                if (!openedStream)
                {
                    Debugger.Break();
                    return (SafeWrapperResult)openedStream;
                }

                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(openedStream.Result);

                _softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                return SafeWrapperResult.S_SUCCESS;
            }

            return new SafeWrapperResult(OperationErrorCode.AccessUnauthorized, "Couldn't retrieve clipboard data");
        }

        public override async Task<SafeWrapperResult> TrySaveData()
        {
            SafeWrapperResult result;

            if (sourceFile == null)
            {
                return ItemIsNotAFileResult;
            }

            BitmapEncoder encoder = null;
            result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                using (IRandomAccessStream fileStream = await sourceFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);

                    encoder.SetSoftwareBitmap(_softwareBitmap);

                    await encoder.FlushAsync();
                }
            }, errorReporter);

            if (!result)
            {
                const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);
                int hresult = result.Exception.HResult;

                result = SafeWrapperRoutines.SafeWrap(async () =>
                {
                    if (hresult == WINCODEC_ERR_UNSUPPORTEDOPERATION)
                    {
                        using (IRandomAccessStream fileStream = await sourceFile.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            encoder.IsThumbnailGenerated = false;

                            await encoder.FlushAsync();
                        }
                    }
                }, errorReporter);
            }

            if (result)
            {
                RaiseOnFileModifiedEvent(this, new FileModifiedEventArgs(sourceFile));
            }

            return result;
        }

        protected override async Task<SafeWrapperResult> SetDataInternal(DataPackageView dataPackage)
        {
            if (dataPackage.Contains(StandardDataFormats.Text)) // Url to file
            {
                SafeWrapper<IRandomAccessStreamWithContentType> openedStream;

                if (dataPackage.Contains(StandardDataFormats.StorageItems)) // Url to StorageFile
                {
                    SafeWrapper<IReadOnlyList<IStorageItem>> items = await SafeWrapperRoutines.SafeWrapAsync(
                        () => dataPackage.GetStorageItemsAsync().AsTask());

                    if (!items)
                    {
                        return items;
                    }

                    StorageFile imageFile = items.Result.As<IEnumerable<IStorageItem>>().First().As<StorageFile>();

                    openedStream = await SafeWrapperRoutines.SafeWrapAsync(
                        () => imageFile.OpenReadAsync().AsTask());

                    if (!openedStream)
                    {
                        Debugger.Break();
                        return (SafeWrapperResult)openedStream;
                    }
                }
                else
                {
                    // Download from url
                    SafeWrapper<string> url = await SafeWrapperRoutines.SafeWrapAsync(
                        () => dataPackage.GetTextAsync().AsTask());

                    if (!url)
                    {
                        return url;
                    }

                    openedStream = await SafeWrapperRoutines.SafeWrapAsync(
                        async () => await (RandomAccessStreamReference.CreateFromUri(new Uri(url))).OpenReadAsync());
                }

                if (!openedStream)
                {
                    Debugger.Break();
                    return (SafeWrapperResult)openedStream;
                }

                SafeWrapperResult result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
                {
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(openedStream.Result);

                    _softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                });

                return result;
            }
            else
            {
                return await base.SetDataInternal(dataPackage);
            }
        }

        protected override async Task<SafeWrapperResult> SetData(IStorageItem item)
        {
            SafeWrapperResult result;
            StorageFile file = item as StorageFile;

            if (file == null)
            {
                return ItemIsNotAFileResult;
            }

            SafeWrapper<Stream> openedStream = await SafeWrapperRoutines.SafeWrapAsync(
                    () => file.OpenStreamForReadAsync());

            if (!openedStream)
            {
                return (SafeWrapperResult)openedStream;
            }

            _dataStream = openedStream.Result;

            if (!openedStream)
            {
                return (SafeWrapperResult)openedStream;
            }

            result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(_dataStream.AsRandomAccessStream());
                _softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            });

            return result;
        }

        protected override async Task<SafeWrapper<StorageFile>> TrySetFileWithExtension()
        {
            SafeWrapper<StorageFile> file = await AssociatedCollection.GetOrCreateNewCollectionFileFromExtension(".png");

            return file;
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            SafeWrapperResult result = null;

            result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                if (_dataStream == null) // Data is pasted as image, load from _softwareBitmap
                {
                    byte[] buffer = await ImagingHelpers.GetBytesFromSoftwareBitmap(_softwareBitmap, BitmapEncoder.PngEncoderId);
                    ContentImage = await ImagingHelpers.ToBitmapAsync(buffer);
                    Array.Clear(buffer, 0, buffer.Length);
                }
                else // Data is read from file or pasted from url, load from data stream
                {
                    BitmapImage image = new BitmapImage();
                    ContentImage = image;
                    _dataStream.Position = 0;
                    await image.SetSourceAsync(_dataStream.AsRandomAccessStream());
                }
            });

            return result;
        }

        #endregion

        #region Public Helpers

        public IReadOnlyList<IStorageItem> ProvideDragData()
        {
            return new List<IStorageItem>() { sourceItem };
        }

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            base.Dispose();

            _dataStream?.Dispose();
            _softwareBitmap?.Dispose();

            _dataStream = null;
            _softwareBitmap = null;
            ContentImage = null;
        }

        #endregion
    }
}
