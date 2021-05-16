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

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class MediaCanvasViewModel : BasePasteCanvasViewModel
    {
        #region Private Members

        private readonly IDynamicPasteCanvasControlView _view;

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

        #endregion

        #region Constructor

        public MediaCanvasViewModel(IDynamicPasteCanvasControlView view)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter)
        {
            this._view = view;
        }

        #endregion

        #region Override

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

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            base.Dispose();

            _ContentMedia?.Dispose();
            ContentMedia = null;
        }

        #endregion
    }
}
