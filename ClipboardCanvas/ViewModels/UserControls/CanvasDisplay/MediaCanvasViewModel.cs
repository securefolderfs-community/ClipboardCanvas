using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.Models;
using ClipboardCanvas.Enums;
using ClipboardCanvas.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media.Core;
using Windows.Storage;
using System.Diagnostics;
using System.IO;
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

        private StorageFile _sourceFile;

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

        private bool _ContentMediaLoad;
        public bool ContentMediaLoad
        {
            get => _ContentMediaLoad;
            set => SetProperty(ref _ContentMediaLoad, value);
        }

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
            if (contentAsReference)
            {
                ReferenceFile referenceFile = await ReferenceFile.GetFile(associatedFile);
                await referenceFile.UpdateReferenceFile(new ReferenceFileData(_sourceFile.Path));

                return SafeWrapperResult.S_SUCCESS;
            }
            else
            {
                // Copy to the collection
                SafeWrapperResult copyResult = await FilesystemOperations.CopyFileAsync(_sourceFile, associatedFile, ReportProgress, cancellationToken);
                if (!copyResult)
                {
                    // Failed
                    Debugger.Break();
                    return copyResult;
                }

                return copyResult;
            }
        }

        protected override async Task<SafeWrapperResult> SetData(DataPackageView dataPackage)
        {
            if (App.AppSettings.UserSettings.CopyLargeItemsDirectlyToCollection)
            {
                contentAsReference = false;
            }
            else
            {
                contentAsReference = true;
            }

            SafeWrapperResult result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                IReadOnlyList<IStorageItem> items = await dataPackage.GetStorageItemsAsync();
                StorageFile mediaFile = (StorageFile)items.First();

                _sourceFile = mediaFile;
            });

            return result;
        }

        protected override async Task<SafeWrapperResult> SetData(StorageFile file)
        {
            if (ReferenceFile.IsReferenceFile(file))
            {
                // Reference file
                contentAsReference = true;

                // Get reference file
                ReferenceFile referenceFile = await ReferenceFile.GetFile(file);

                // Set the _sourceFile
                _sourceFile = referenceFile.ReferencedFile;
            }
            else
            {
                // In collection file
                contentAsReference = false;

                associatedFile = file;
            }
            return await Task.FromResult(SafeWrapperResult.S_SUCCESS);
        }

        protected override async Task<SafeWrapper<StorageFile>> TrySetFileWithExtension()
        {
            SafeWrapper<StorageFile> file;

            file = await AssociatedContainer.GetEmptyFileToWrite(Path.GetExtension(_sourceFile.Path), Path.GetFileNameWithoutExtension(_sourceFile.Path));

            return file;
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            ContentMediaLoad = true;

            if (contentAsReference)
            {
                ContentMedia = MediaSource.CreateFromStorageFile(_sourceFile);
            }   
            else
            {
                ContentMedia = MediaSource.CreateFromStorageFile(associatedFile);
            }

            return await Task.FromResult(SafeWrapperResult.S_SUCCESS);
        }

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            base.Dispose();

            ContentMedia?.Dispose();
            ContentMedia = null;
        }

        #endregion
    }
}
