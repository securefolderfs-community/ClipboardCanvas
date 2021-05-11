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
using ClipboardCanvas.Extensions;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class WebViewCanvasViewModel : BasePasteCanvasViewModel
    {
        #region Private Members

        private readonly IDynamicPasteCanvasControlView _view;

        #endregion

        #region Protected Members

        protected override ICollectionsContainerModel AssociatedContainer => _view?.CollectionContainer;

        #endregion

        #region Constructor

        public WebViewCanvasViewModel(IDynamicPasteCanvasControlView view)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter)
        {
            this._view = view;
        }

        #endregion

        #region Public Properties

        public static List<string> Extensions => new List<string>() {
            ".html", ".htm",
        };

        private string _TextHtml;
        public string TextHtml
        {
            get => _TextHtml;
            set => SetProperty(ref _TextHtml, value);
        }

        #endregion

        #region Override

        protected override async Task<SafeWrapperResult> SetData(DataPackageView dataPackage)
        {
            return await Task.FromResult(SafeWrapperResult.S_SUCCESS);
        }

        public override async Task<SafeWrapperResult> TrySaveData()
        {
            SafeWrapperResult result = await SafeWrapperRoutines.SafeWrapAsync(() => FileIO.WriteTextAsync(associatedFile, TextHtml).AsTask());
            return result;
        }

        protected override async Task<SafeWrapperResult> SetData(IStorageFile file)
        {
            string text = await FileIO.ReadTextAsync(file);
            _TextHtml = text;

            return await Task.FromResult(SafeWrapperResult.S_SUCCESS);
        }

        protected override async Task<SafeWrapper<StorageFile>> TrySetFileWithExtension()
        {
            SafeWrapper<StorageFile> file = await AssociatedContainer.GetEmptyFileToWrite(".html");
           
            return file;
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            if (!string.IsNullOrEmpty(TextHtml))
            {
                OnPropertyChanged(nameof(TextHtml));
                return await Task.FromResult(SafeWrapperResult.S_SUCCESS);
            }
            else
            {
                SafeWrapper<string> text = await SafeWrapperRoutines.SafeWrapAsync(async () => await FileIO.ReadTextAsync(sourceFile));

                if (text)
                {
                    TextHtml = text;
                }

                return (SafeWrapperResult)text;
            }
        }

        public override async Task<IEnumerable<SuggestedActionsControlItemViewModel>> GetSuggestedActions()
        {
            return null;
        }

        protected override bool CanPasteAsReference()
        {
            return true;
        }

        #endregion
    }
}
