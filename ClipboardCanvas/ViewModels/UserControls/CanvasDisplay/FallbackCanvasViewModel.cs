using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using System.IO;
using Windows.Storage;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.Models;
using ClipboardCanvas.Enums;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.ReferenceItems;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.EventArguments.CanvasControl;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class FallbackCanvasViewModel : BasePasteCanvasViewModel
    {
        #region Private Members

        private readonly IDynamicPasteCanvasControlView _view;

        private StorageItemThumbnail _thumbnail;

        #endregion

        #region Protected Members

        protected override ICollectionsContainerModel AssociatedContainer => _view?.CollectionContainer;

        #endregion

        #region Public Properties

        private string _FileName;
        public string FileName
        {
            get => _FileName;
            set => SetProperty(ref _FileName, value);
        }

        private string _FilePath;
        public string FilePath
        {
            get => _FilePath;
            set => SetProperty(ref _FilePath, value);
        }

        private DateTime _DateCreated;
        public DateTime DateCreated
        {
            get => _DateCreated;
            set => SetProperty(ref _DateCreated, value);
        }

        private DateTime _DateModified;
        public DateTime DateModified
        {
            get => _DateModified;
            set => SetProperty(ref _DateModified, value);
        }

        private BitmapImage _FileIcon;
        public BitmapImage FileIcon
        {
            get => _FileIcon; 
            set => SetProperty(ref _FileIcon, value);
        }

        private bool _CanDrag;
        public bool CanDrag
        {
            get => _CanDrag;
            set => SetProperty(ref _CanDrag, value);
        }

        #endregion

        #region Constructor

        public FallbackCanvasViewModel(IDynamicPasteCanvasControlView view, CanvasPreviewMode canvasMode)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, canvasMode)
        {
            this._view = view;
        }

        #endregion

        #region Override

        public override async Task<SafeWrapperResult> TrySaveData()
        {
            return await Task.FromResult(SafeWrapperResult.S_SUCCESS);
        }

        protected override async Task<SafeWrapperResult> SetData(StorageFile file)
        {
            // Read file properties

            if (file == null)
            {
                return new SafeWrapperResult(OperationErrorCode.InvalidArgument, new ArgumentNullException(), "The file is null.");
            }

            this._FileName = Path.GetFileName(file.Path);
            this._FilePath = file.Path;
            this._DateCreated = file.DateCreated.DateTime;

            var properties = await file.GetBasicPropertiesAsync();
            this._DateModified = properties.DateModified.DateTime;

            _thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem);

            _FileIcon = new BitmapImage();
            await _FileIcon.SetSourceAsync(_thumbnail);

            return SafeWrapperResult.S_SUCCESS;
        }

        protected override async Task<SafeWrapperResult> SetDataInternal(DataPackageView dataPackage)
        {
            SafeWrapperResult result = await base.SetDataInternal(dataPackage);

            if (result && dataPackage.Contains(StandardDataFormats.StorageItems))
            {
                // It was an item, otherwise SetData() is called

                return await SetData(sourceFile as StorageFile);
            }

            return result;
        }

        protected override async Task<SafeWrapperResult> SetData(DataPackageView dataPackage)
        {
            return await Task.FromResult(SafeWrapperResult.S_SUCCESS);
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(FilePath));
            OnPropertyChanged(nameof(DateCreated));
            OnPropertyChanged(nameof(DateModified));
            OnPropertyChanged(nameof(FileIcon));

            return await Task.FromResult(SafeWrapperResult.S_SUCCESS);
        }

        protected override async Task<SafeWrapper<StorageFile>> TrySetFileWithExtension()
        {
            return await Task.FromResult(new SafeWrapper<StorageFile>(associatedFile, SafeWrapperResult.S_SUCCESS));
        }

        public override async Task<IEnumerable<SuggestedActionsControlItemViewModel>> GetSuggestedActions()
        {
            return await Task.FromResult<IEnumerable<SuggestedActionsControlItemViewModel>>(null);
        }

        protected override void OnCanvasModeChanged(CanvasPreviewMode canvasMode)
        {
            switch (canvasMode)
            {
                case CanvasPreviewMode.PreviewOnly:
                    {
                        CanDrag = false;
                        break;
                    }

                case CanvasPreviewMode.InteractionAndPreview:
                    {
                        CanDrag = true;
                        break;
                    }

                case CanvasPreviewMode.WriteAndPreview:
                    {
                        CanDrag = false;
                        break;
                    }
            }
        }

        #endregion

        #region Public Helpers

        public IReadOnlyList<IStorageItem> ProvideDragData()
        {
            return new List<IStorageItem>() { sourceFile };
        }

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            base.Dispose();

            _thumbnail?.Dispose();
        }

        #endregion
    }
}
