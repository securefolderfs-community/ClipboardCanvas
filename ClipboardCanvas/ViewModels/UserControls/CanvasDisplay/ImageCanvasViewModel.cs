using System;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using Windows.Storage.Streams;
using System.Collections.Generic;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.DataModels.ContentDataModels;
using ClipboardCanvas.CanavsPasteModels;
using ClipboardCanvas.Models;
using ClipboardCanvas.Contexts.Operations;
using ClipboardCanvas.Extensions;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public sealed class ImageCanvasViewModel : BaseCanvasViewModel, IDragDataProviderModel
    {
        #region Members

        private SoftwareBitmap _softwareBitmap;

        #endregion

        #region Properties

        private ImagePasteModel ImagePasteModel => canvasPasteModel as ImagePasteModel;

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

        public ImageCanvasViewModel(IBaseCanvasPreviewControlView view, BaseContentTypeModel contentType)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, contentType, view)
        {
        }

        #endregion

        #region Override

        protected override async Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
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
                _softwareBitmap = await bitmapDecoder.GetSoftwareBitmapAsync();
            });

            openedStream.Result.Dispose();

            return result;
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            return await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                byte[] buffer;

                if (ImagePasteModel != null && ImagePasteModel?.SoftwareBitmap != null) // Data is pasted, use ImagePasteModel.SoftwareBitmap
                {
                    buffer = await ImagingHelpers.GetBytesFromSoftwareBitmap(ImagePasteModel.SoftwareBitmap, BitmapEncoder.PngEncoderId);
                }
                else // Data is read from file, use local _softwareBitmap
                {
                    buffer = await ImagingHelpers.GetBytesFromSoftwareBitmap(_softwareBitmap, BitmapEncoder.PngEncoderId);
                }

                ContentImage = await ImagingHelpers.ToBitmapAsync(buffer);
                Array.Clear(buffer, 0, buffer.Length);
            });
        }

        protected override IPasteModel SetCanvasPasteModel()
        {
            return new ImagePasteModel(AssociatedCollection, new StatusCenterOperationReceiver());
        }

        #endregion

        #region Public Helpers

        public async Task<IReadOnlyList<IStorageItem>> GetDragData()
        {
            return (await sourceItem).ToListSingle();
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
