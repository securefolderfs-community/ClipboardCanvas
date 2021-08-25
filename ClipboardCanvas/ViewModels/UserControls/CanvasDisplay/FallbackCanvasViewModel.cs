using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.DataModels.ContentDataModels;
using ClipboardCanvas.CanavsPasteModels;
using ClipboardCanvas.Contexts.Operations;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.Helpers;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class FallbackCanvasViewModel : BaseCanvasViewModel
    {
        #region Members

        private StorageItemThumbnail _itemThumbnail;

        private bool _isFolder;

        #endregion

        #region Properties

        private FallbackPasteModel FallbackPasteModel => canvasPasteModel as FallbackPasteModel;

        private string _FileName;
        public string FileName
        {
            get => FallbackPasteModel?.FileName ?? _FileName;
        }

        private string _FilePath;
        public string FilePath
        {
            get => FallbackPasteModel?.FilePath ?? _FilePath;
        }

        private DateTime _DateCreated;
        public DateTime DateCreated
        {
            get => FallbackPasteModel?.DateCreated ?? _DateCreated;
        }

        private DateTime _DateModified;
        public DateTime DateModified
        {
            get => FallbackPasteModel?.DateModified ?? _DateModified;
        }

        private BitmapImage _FileIcon;
        public BitmapImage FileIcon
        {
            get => FallbackPasteModel?.FileIcon ?? _FileIcon;
        }

        #endregion

        #region Constructor

        public FallbackCanvasViewModel(IBaseCanvasPreviewControlView view, BaseContentTypeModel contentType)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, contentType, view)
        {
        }

        #endregion

        #region Override

        protected override async Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
        {
            // Read file properties
            if (item is StorageFile file)
            {
                _itemThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem);
            }
            else if (item is StorageFolder folder)
            {
                _isFolder = true;
                _itemThumbnail = await folder.GetThumbnailAsync(ThumbnailMode.SingleItem);
            }

            this._FileName = item.Name;
            this._FilePath = item.Path;
            this._DateCreated = item.DateCreated.DateTime;

            var properties = await item.GetBasicPropertiesAsync();
            this._DateModified = properties.DateModified.DateTime;

            _FileIcon = new BitmapImage();
            await FileIcon.SetSourceAsync(_itemThumbnail);

            return SafeWrapperResult.SUCCESS;
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(FilePath));
            OnPropertyChanged(nameof(DateCreated));
            OnPropertyChanged(nameof(DateModified));
            OnPropertyChanged(nameof(FileIcon));

            return await Task.FromResult(SafeWrapperResult.SUCCESS);
        }

        protected override IPasteModel SetCanvasPasteModel()
        {
            return new FallbackPasteModel(associatedCollection, new StatusCenterOperationReceiver());
        }

        protected override bool CheckCanPasteReference()
        {
            return !_isFolder;
        }

        protected override async Task OnReferencePasted()
        {
            string newPath = await CanvasHelpers.SafeGetCanvasItemPath(canvasItem);

            this._FilePath = newPath;
            this._FileName = Path.GetFileName(newPath);

            this.FallbackPasteModel?.UpdatePathProperty(newPath);

            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(FilePath));
        }

        #endregion

        #region Public Helpers

        public async Task<IReadOnlyList<IStorageItem>> ProvideDragData()
        {
            return (await sourceItem).ToListSingle();
        }

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            base.Dispose();

            _itemThumbnail?.Dispose();

            _itemThumbnail = null;
            _FileIcon = null;
        }

        #endregion
    }
}
