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
using ClipboardCanvas.CanavsPasteModels;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class MarkdownCanvasViewModel : BaseCanvasViewModel
    {
        #region Public Properties

        protected override IPasteModel CanvasPasteModel => null;

        public static List<string> Extensions => new List<string>() {
            ".md", ".markdown",
        };

        private string _TextMarkdown;
        public string TextMarkdown
        {
            get => _TextMarkdown;
            set => SetProperty(ref _TextMarkdown, value);
        }

        #endregion

        #region Constructor

        public MarkdownCanvasViewModel(IBaseCanvasPreviewControlView view)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, new MarkdownContentType(), view)
        {
        }

        #endregion

        #region Override

        public override async Task<SafeWrapperResult> TrySaveData()
        {
            SafeWrapperResult result = await FilesystemOperations.WriteFileText(await sourceFile, TextMarkdown);

            return result;
        }

        protected override async Task<SafeWrapperResult> SetDataFromExistingFile(IStorageItem item)
        {
            if (item is not StorageFile file)
            {
                return ItemIsNotAFileResult;
            }

            SafeWrapper<string> text = await FilesystemOperations.ReadFileText(file);

            this._TextMarkdown = text;

            return text;
        }

        protected override async Task<SafeWrapperResult> SetData(DataPackageView dataPackage)
        {
            SafeWrapper<string> text = await SafeWrapperRoutines.SafeWrapAsync(
                           () => dataPackage.GetTextAsync().AsTask());

            if (!text)
            {
                Debugger.Break();
                return (SafeWrapperResult)text;
            }

            _TextMarkdown = text;

            return (SafeWrapperResult)text;
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            OnPropertyChanged(nameof(TextMarkdown));

            return await Task.FromResult(SafeWrapperResult.SUCCESS);
        }

        protected override async Task<SafeWrapper<CollectionItemViewModel>> TrySetFileWithExtension()
        {
            SafeWrapper<CollectionItemViewModel> itemViewModel = await associatedCollection.CreateNewCollectionItemFromExtension(".md");

            return itemViewModel;
        }

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            base.Dispose();

            TextMarkdown = null;
        }

        #endregion
    }
}
