using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.EventArguments.CanvasControl;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class TextCanvasViewModel : BasePasteCanvasViewModel
    {
        #region Private Members

        private IDynamicPasteCanvasControlView _view;

        #endregion

        #region Protected Properties

        protected override ICollectionsContainerModel AssociatedContainer => _view?.CollectionContainer;

        #endregion

        #region Public Properties

        private bool _ContentTextLoad;
        public bool ContentTextLoad
        {
            get => _ContentTextLoad;
            private set => SetProperty(ref _ContentTextLoad, value);
        }

        private string _ContentText;
        public string ContentText
        {
            get => _ContentText;
            set => SetProperty(ref _ContentText, value);
        }

        public static List<string> Extensions => new List<string>() {
            ".txt"
        };

        #endregion

        #region Constructor

        public TextCanvasViewModel(IDynamicPasteCanvasControlView view)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter) // TODO: Use custom exception reporter
        {
            this._view = view;
        }

        #endregion

        #region Override

        protected override async Task<SafeWrapperResult> SetData(DataPackageView dataPackage)
        {
            SafeWrapperResult result;
            SafeWrapper<string> text = await SafeWrapperRoutines.SafeWrapAsync(
                           () => dataPackage.GetTextAsync().AsTask());

            result = text;
            if (!result)
            {
                Debugger.Break();
                return result;
            }

            dataStream = StreamHelpers.CreateStreamFromString(text);

            return result;
        }

        protected override async Task<SafeWrapper<StorageFile>> TrySetFileWithExtension()
        {
            SafeWrapper<StorageFile> file;

            file = await AssociatedContainer.GetEmptyFileToWrite(".txt");

            return file;
        }

        public override async Task<SafeWrapperResult> TrySaveData()
        {
            SafeWrapperResult result;

            result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                dataStream.Position = 0L;
                using (fileStream = await associatedFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await dataStream.CopyToAsync(fileStream.AsStreamForWrite());
                }
            }, errorReporter);

            return result;
        }

        protected override Task<SafeWrapperResult> TryFetchDataToView()
        {
            ContentTextLoad = true;

            SafeWrapperResult result = SafeWrapperRoutines.SafeWrap(() =>
            {
                StreamReader reader = new StreamReader(dataStream);
                ContentText = reader.ReadToEnd();
            });

            return Task.FromResult(result);
        }

        #endregion

        #region Public Helpers

        public static async Task<bool> CanLoadAsText(StorageFile file)
        {
            // Check if exceeds maximum fileSize or is zero
            long fileSize = await file.GetFileSize();
            if (fileSize > Constants.CanvasContent.FALLBACK_TEXTLOAD_MAX_FILESIZE || fileSize == 0L)
            {
                return false;
            }

            try
            {
                // Check if file is binary
                string text = await FileIO.ReadTextAsync(file);
                if (text.Contains("\0\0\0\0"))
                {
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        #endregion
    }
}
