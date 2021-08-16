using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using Windows.Storage.Streams;

using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers;

namespace ClipboardCanvas.ViewModels.UserControls.SimpleCanvasDisplay
{
    public sealed class ThumbnailSimpleCanvasViewModel : BaseReadOnlyCanvasViewModel
    {
        #region Private Members

        private IRandomAccessStream _thumbnail;

        #endregion

        #region Public Properties

        private BitmapImage _FileIcon;
        public BitmapImage FileIcon
        {
            get => _FileIcon;
            set => SetProperty(ref _FileIcon, value);
        }

        #endregion

        #region Constructor

        public ThumbnailSimpleCanvasViewModel(IBaseCanvasPreviewControlView view)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, new ImageContentType(), view)
        {
        }

        #endregion

        #region Override

        protected override async Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
        {
            // Get thumbnail for Infinite Canvas
            StorageFolder folder = item as StorageFolder;
            if (folder != null && FilesystemHelpers.IsPathEqualExtension(item.Path, Constants.FileSystem.INFINITE_CANVAS_EXTENSION))
            {
                string canvasPreviewImageFileName = Constants.FileSystem.INFINITE_CANVAS_PREVIEW_IMAGE_FILENAME;
                string canvasPreviewImageFilePath = Path.Combine(folder.Path, canvasPreviewImageFileName);

                SafeWrapper<StorageFile> canvasPreviewImageFileResult = await StorageHelpers.ToStorageItemWithError<StorageFile>(canvasPreviewImageFilePath);
                if (!canvasPreviewImageFileResult)
                {
                    return canvasPreviewImageFileResult;
                }

                SafeWrapper<IRandomAccessStream> transparentThumbnailResult = await SafeWrapperRoutines.SafeWrapAsync(() =>
                    canvasPreviewImageFileResult.Result.GetTransparentThumbnail(ThumbnailMode.SingleItem, Constants.UI.CanvasContent.SIMPLE_CANVAS_THUMBNAIL_SIZE));

                if (!transparentThumbnailResult)
                {
                    transparentThumbnailResult.Result?.Dispose();
                    return transparentThumbnailResult;
                }

                _thumbnail = transparentThumbnailResult.Result;
            }
            // Get thumbnail for file
            else if (item is StorageFile file)
            {
                _thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, Constants.UI.CanvasContent.SIMPLE_CANVAS_THUMBNAIL_SIZE);
            }
            // Get thumbnail for folder
            else if (folder != null)
            {
                _thumbnail = await folder.GetThumbnailAsync(ThumbnailMode.SingleItem, Constants.UI.CanvasContent.SIMPLE_CANVAS_THUMBNAIL_SIZE, ThumbnailOptions.ResizeThumbnail);
            }
            else
            {
                return ItemIsNotAFileResult;
            }

            _FileIcon = new BitmapImage();
            SafeWrapperResult result = await SafeWrapperRoutines.SafeWrapAsync(() =>
                _FileIcon.SetSourceAsync(_thumbnail).AsTask());

            return result;
        }

        protected override Task<SafeWrapperResult> TryFetchDataToView()
        {
            OnPropertyChanged(nameof(FileIcon));

            return Task.FromResult(SafeWrapperResult.SUCCESS);
        }

        protected override void RefreshContextMenuItems()
        {
            return;
        }

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            base.Dispose();

            _thumbnail?.Dispose();

            _thumbnail = null;
            _FileIcon = null;
        }

        #endregion
    }
}
