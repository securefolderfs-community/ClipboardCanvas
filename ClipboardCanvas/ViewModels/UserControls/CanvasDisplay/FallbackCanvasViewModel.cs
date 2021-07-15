using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using System.IO;
using Windows.Storage;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.Enums;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.DataModels.PastedContentDataModels;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class FallbackCanvasViewModel : BaseCanvasViewModel
    {
        #region Private Members

        private StorageItemThumbnail _thumbnail;

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

        #endregion

        #region Constructor

        public FallbackCanvasViewModel(IBaseCanvasPreviewControlView view)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, new FallbackContentType(), view)
        {
        }

        #endregion

        #region Override

        public override async Task<SafeWrapperResult> TrySaveData()
        {
            return await Task.FromResult(SafeWrapperResult.S_SUCCESS);
        }

        protected override async Task<SafeWrapperResult> SetData(IStorageItem item)
        {
            // Read file properties
            if (item is StorageFile file)
            {
                _thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem);
            }
            else if (item is StorageFolder folder)
            {
                _thumbnail = await folder.GetThumbnailAsync(ThumbnailMode.SingleItem);
            }

            this._FileName = Path.GetFileName(item.Path);
            this._FilePath = item.Path;
            this._DateCreated = item.DateCreated.DateTime;

            var properties = await item.GetBasicPropertiesAsync();
            this._DateModified = properties.DateModified.DateTime;

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
