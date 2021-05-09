using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.IO;
using Windows.Storage.Streams;
using System.Diagnostics;
using Windows.UI.Xaml.Media;
using System.Linq;
using System.Collections.Generic;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

using ClipboardCanvas.Extensions;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Enums;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.Helpers.Filesystem;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public sealed class ImageCanvasViewModel : BasePasteCanvasViewModel
    {
        #region Private Members

        private readonly IDynamicPasteCanvasControlView _view;

        private SoftwareBitmap _softwareBitmap;

        private string imageFileName;

        #endregion

        #region Protected Properties

        protected override ICollectionsContainerModel AssociatedContainer => _view?.CollectionContainer;

        #endregion

        #region Public Properties

        public static List<string> Extensions => new List<string>() {
            ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tiff", ".ico", ".svg", ".webp"
        };

        private bool _ContentImageLoad;
        public bool ContentImageLoad
        {
            get => _ContentImageLoad;
            private set => SetProperty(ref _ContentImageLoad, value);
        }

        private ImageSource _ContentImage;
        public ImageSource ContentImage
        {
            get => _ContentImage;
            set => SetProperty(ref _ContentImage, value);
        }

        #endregion

        #region Constructor

        public ImageCanvasViewModel(IDynamicPasteCanvasControlView view)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter) // TODO: Use custom exception reporter
        {
            this._view = view;
        }

        #endregion

        #region Override

        protected override async Task<SafeWrapperResult> SetData(DataPackageView dataPackage)
        {
            if (dataPackage.Contains(StandardDataFormats.StorageItems))
            {
                SafeWrapper<IReadOnlyList<IStorageItem>> items = await SafeWrapperRoutines.SafeWrapAsync(
                    () => dataPackage.GetStorageItemsAsync().AsTask());

                if (!items)
                {
                    Debugger.Break();
                    return (SafeWrapperResult)items;
                }

                sourceFile = items.Result.As<IEnumerable<IStorageItem>>().First().As<StorageFile>();
                imageFileName = Path.GetFileName(sourceFile.Path);

                SafeWrapper<Stream> openedStream = await SafeWrapperRoutines.SafeWrapAsync(
                    () => sourceFile.OpenStreamForReadAsync());

                if (!openedStream)
                {
                    return (SafeWrapperResult)openedStream;
                }

                dataStream = openedStream.Result;

                return SafeWrapperResult.S_SUCCESS;
            }
            else
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
        }

        public override async Task<SafeWrapperResult> TrySaveData()
        {
            SafeWrapperResult result;

            if (sourceFile != null)
            {
                // Copy to the collection
                SafeWrapperResult copyResult = await FilesystemOperations.CopyFileAsync(sourceFile, associatedFile, ReportProgress, cancellationToken);

                return copyResult;
            }

            BitmapEncoder encoder = null;
            result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                using (IRandomAccessStream fileStream = await associatedFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);

                    encoder.SetSoftwareBitmap(_softwareBitmap);

                    await encoder.FlushAsync();
                }
            }, errorReporter);

            if (!result)
            {
                const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);
                int hresult = result.Details.innerException.HResult;

                result = SafeWrapperRoutines.SafeWrap(async () =>
                {
                    if (hresult == WINCODEC_ERR_UNSUPPORTEDOPERATION)
                    {
                        using (fileStream = await associatedFile.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            encoder.IsThumbnailGenerated = false;

                            await encoder.FlushAsync();
                        }
                    }
                }, errorReporter);
            }

            if (result)
            {
                RaiseOnFileModifiedEvent(this, new FileModifiedEventArgs(associatedFile, AssociatedContainer));
            }

            return result;
        }

        protected override async Task<SafeWrapperResult> SetData(StorageFile file)
        {
            SafeWrapperResult result;

            if (cancellationToken.IsCancellationRequested)
            {
                return new SafeWrapperResult(OperationErrorCode.InProgress, "The operation was canceled");
            }

            dataStream = new MemoryStream();
            result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                using (fileStream = await file?.OpenAsync(FileAccessMode.Read))
                {
                    await fileStream.AsStreamForRead().CopyToAsync(dataStream.AsOutputStream().AsStreamForWrite());
                }
            }, errorReporter);

            if (!result)
            {
                return result;
            }

            result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(dataStream.AsRandomAccessStream());
                _softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            });

            return result;
        }

        protected override async Task<SafeWrapper<StorageFile>> TrySetFileWithExtension()
        {
            SafeWrapper<StorageFile> file;

            if (!string.IsNullOrEmpty(imageFileName))
            {
                string extension = Path.GetExtension(imageFileName);
                file = await AssociatedContainer.GetEmptyFileToWrite(extension, Path.GetFileNameWithoutExtension(imageFileName));
            }
            else
            {
                file = await AssociatedContainer.GetEmptyFileToWrite(".png");
            }

            return file;
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            ContentImageLoad = true;
            SafeWrapperResult result = null;

            result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                if (dataStream == null) // Data is pasted, load from _softwareBitmap
                {
                    byte[] buffer = await ImagingHelpers.EncodedBytes(_softwareBitmap, BitmapEncoder.PngEncoderId);
                    ContentImage = await ImagingHelpers.ToBitmapAsync(buffer);
                    Array.Clear(buffer, 0, buffer.Length);
                }
                else // Data is read from file, load from data stream
                {
                    BitmapImage image = new BitmapImage();
                    ContentImage = image;
                    dataStream.Position = 0;
                    await image.SetSourceAsync(dataStream.AsRandomAccessStream());
                }
            });

            return result;
        }

        protected override bool CanPasteAsReference()
        {
            return sourceFile != null;
        }

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            base.Dispose();

            _softwareBitmap?.Dispose();
            _softwareBitmap = null;
            ContentImage = null;
        }

        #endregion
    }
}
