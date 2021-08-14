using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.CanavsPasteModels;
using ClipboardCanvas.Contexts.Operations;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class MarkdownCanvasViewModel : BaseCanvasViewModel
    {
        #region Public Properties

        private MarkdownPasteModel MarkdownPasteModel => canvasPasteModel as MarkdownPasteModel;

        public static List<string> Extensions => new List<string>() {
            ".md", ".markdown",
        };

        private string _MarkdownText;
        public string MarkdownText
        {
            get => MarkdownPasteModel?.MarkdownText ?? _MarkdownText;
        }

        #endregion

        #region Constructor

        public MarkdownCanvasViewModel(IBaseCanvasPreviewControlView view, BaseContentTypeModel contentType)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, contentType, view)
        {
        }

        #endregion

        #region Override

        protected override async Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
        {
            if (item is not StorageFile file)
            {
                return ItemIsNotAFileResult;
            }

            SafeWrapper<string> text = await FilesystemOperations.ReadFileText(file);

            this._MarkdownText = text;

            return text;
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            OnPropertyChanged(nameof(MarkdownText));

            return await Task.FromResult(SafeWrapperResult.SUCCESS);
        }

        protected override IPasteModel SetCanvasPasteModel()
        {
            return new MarkdownPasteModel(associatedCollection, new StatusCenterOperationReceiver());
        }

        #endregion
    }
}
