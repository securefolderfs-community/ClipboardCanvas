using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.Core;
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
using ClipboardCanvas.DataModels.PastedContentDataModels;
using System.Threading;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class MediaCanvasViewModel : BasePasteCanvasViewModel
    {
        #region Private Members

        private readonly IDynamicPasteCanvasControlView _view;

        private MediaContentType _mediaContentType => contentType as MediaContentType;

        private TimeSpan _Position
        {
            get => ControlView.Position;
            set => ControlView.Position = value;
        }

        #endregion

        #region Protected Members

        protected override ICollectionsContainerModel AssociatedContainer => _view?.CollectionContainer;

        #endregion

        #region Public Properties

        public static List<string> Extensions => new List<string>() {
            // Video
            ".mp4", ".webm", ".ogg", ".mov", ".qt", ".mp4", ".m4v", ".mp4v", ".3g2", ".3gp2", ".3gp", ".3gpp", ".mkv",
            // Audio
            ".mp3", ".m4a", ".wav", ".wma", ".aac", ".adt", ".adts", ".cda",
        };

        private MediaSource _ContentMedia;
        public MediaSource ContentMedia
        {
            get => _ContentMedia;
            set => SetProperty(ref _ContentMedia, value);
        }

        public IMediaCanvasControlView ControlView { get; set; }

        #endregion

        #region Constructor

        public MediaCanvasViewModel(IDynamicPasteCanvasControlView view, CanvasPreviewMode canvasMode)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, new MediaContentType(), canvasMode)
        {
            this._view = view;
        }

        #endregion

        #region Override

        public override async Task<SafeWrapperResult> TryLoadExistingData(ICollectionsContainerItemModel itemData, CancellationToken cancellationToken)
        {
            SafeWrapperResult result = await base.TryLoadExistingData(itemData, cancellationToken);

            if (result)
            {
                UpdatePlaybackPosition();
            }

            return result;
        }

        public override async Task<SafeWrapperResult> TrySaveData()
        {
            return await Task.FromResult(SafeWrapperResult.S_SUCCESS);
        }

        protected override async Task<SafeWrapperResult> SetData(DataPackageView dataPackage)
        {
            return await Task.FromResult(SafeWrapperResult.S_SUCCESS);
        }

        protected override async Task<SafeWrapperResult> SetData(StorageFile file)
        {
            return await Task.FromResult(SafeWrapperResult.S_SUCCESS);
        }

        protected override async Task<SafeWrapper<StorageFile>> TrySetFileWithExtension()
        {
            return await Task.FromResult(new SafeWrapper<StorageFile>(associatedFile, SafeWrapperResult.S_SUCCESS));
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            ContentMedia = MediaSource.CreateFromStorageFile(sourceFile);

            return await Task.FromResult(SafeWrapperResult.S_SUCCESS);
        }

        protected override void OnReferencePasted()
        {
            // Change the source
            _ContentMedia = MediaSource.CreateFromStorageFile(sourceFile);
        }

        #endregion

        #region Public Helpers

        public void UpdatePlaybackPosition()
        {
            if (_Position != null)
            {
                this._Position = _mediaContentType.savedPosition;
            }
        }

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            ICollectionsContainerItemModel associatedContainerItem = AssociatedContainer.Items.Where((item) => item.File == associatedFile).FirstOrDefault();
            if (associatedContainerItem?.ContentType is MediaContentType mediaContentType)
            {
                mediaContentType.savedPosition = _Position;
            }

            base.Dispose();

            ContentMedia?.Dispose();
            ContentMedia = null;
        }

        #endregion
    }
}
