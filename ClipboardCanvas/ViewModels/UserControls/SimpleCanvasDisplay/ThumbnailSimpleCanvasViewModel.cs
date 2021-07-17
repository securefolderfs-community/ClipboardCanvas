using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.ViewModels.ContextMenu;
using System.Collections.Generic;

namespace ClipboardCanvas.ViewModels.UserControls.SimpleCanvasDisplay
{
    public sealed class ThumbnailSimpleCanvasViewModel : BaseReadOnlyCanvasViewModel
    {
        #region Private Members

        private StorageItemThumbnail _thumbnail;

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

        protected override async Task<SafeWrapperResult> SetData(IStorageItem item)
        {
            if (item is StorageFile file)
            {
                _thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, Constants.UI.CanvasContent.SIMPLE_CANVAS_THUMBNAIL_SIZE);
            }
            else if (item is StorageFolder folder)
            {
                _thumbnail = await folder.GetThumbnailAsync(ThumbnailMode.SingleItem, Constants.UI.CanvasContent.SIMPLE_CANVAS_THUMBNAIL_SIZE);
            }
            else
            {
                return ItemIsNotAFileResult;
            }

            _FileIcon = new BitmapImage();
            await _FileIcon.SetSourceAsync(_thumbnail);

            return SafeWrapperResult.S_SUCCESS;
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            OnPropertyChanged(nameof(FileIcon));

            return await Task.FromResult(SafeWrapperResult.S_SUCCESS);
        }

        protected override void RefreshContextMenuItems()
        {
            return;
        }

        #endregion
    }
}
