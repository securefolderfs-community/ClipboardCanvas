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

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class MarkdownCanvasViewModel : BaseCanvasViewModel
    {
        #region Private Members

        private readonly IDynamicCanvasControlView _view;

        #endregion

        #region Protected Members

        protected override ICollectionModel AssociatedCollection => _view?.CollectionModel;

        #endregion

        #region Public Properties

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

        public MarkdownCanvasViewModel(IDynamicCanvasControlView view, CanvasPreviewMode canvasMode)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, new MarkdownContentType(), canvasMode)
        {
            this._view = view;
        }

        #endregion

        #region Override

        public override async Task<SafeWrapperResult> TrySaveData()
        {
            SafeWrapperResult result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                await FileIO.WriteTextAsync(sourceFile, TextMarkdown);
            }, errorReporter);

            return result;
        }

        protected override async Task<SafeWrapperResult> SetData(StorageFile file)
        {
            SafeWrapper<string> text = await SafeWrapperRoutines.SafeWrapAsync(async () => await FileIO.ReadTextAsync(file));

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

            return await Task.FromResult(SafeWrapperResult.S_SUCCESS);
        }

        protected override async Task<SafeWrapper<StorageFile>> TrySetFileWithExtension()
        {
            SafeWrapper<StorageFile> file = await AssociatedCollection.GetEmptyFileToWrite(".md");

            return file;
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
