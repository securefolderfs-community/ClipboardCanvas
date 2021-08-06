using System;
using System.Threading.Tasks;
using Windows.Storage;

using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.Helpers.Filesystem;

namespace ClipboardCanvas.ViewModels.UserControls.SimpleCanvasDisplay
{
    public sealed class TextSimpleCanvasViewModel : BaseReadOnlyCanvasViewModel
    {
        #region Public Properties

        private string _ContentText;
        public string ContentText
        {
            get => _ContentText;
            set => SetProperty(ref _ContentText, value);
        }

        #endregion

        #region Constructor

        public TextSimpleCanvasViewModel(IBaseCanvasPreviewControlView view)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, new TextContentType(), view)
        {
        }

        #endregion

        #region Override

        protected override async Task<SafeWrapperResult> SetDataFromExistingFile(IStorageItem item)
        {
            if (item is not StorageFile file)
            {
                return ItemIsNotAFileResult;
            }

            SafeWrapper<string> text = await FilesystemOperations.ReadFileText(file);

            this._ContentText = text;

            return text;
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            OnPropertyChanged(nameof(ContentText));

            return await Task.FromResult(SafeWrapperResult.SUCCESS);
        }

        protected override void RefreshContextMenuItems()
        {
            return;
        }

        #endregion
    }
}
