using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Microsoft.Toolkit.Mvvm.Input;
using System.Diagnostics;
using System.Collections.Generic;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.ViewModels.ContextMenu;
using ClipboardCanvas.CanavsPasteModels;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class TextCanvasViewModel : BaseCanvasViewModel
    {
        #region Public Properties

        protected override IPasteModel CanvasPasteModel => null;

        private string _ContentText;
        public string ContentText
        {
            get => _ContentText;
            set => SetProperty(ref _ContentText, value);
        }

        public static List<string> Extensions => new List<string>() {
            ".txt"
        };

        public ITextCanvasControlView ControlView { get; set; }

        #endregion

        #region Constructor

        public TextCanvasViewModel(IBaseCanvasPreviewControlView view)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, new TextContentType(), view)
        {
        }

        #endregion

        #region Override

        protected override async Task<SafeWrapperResult> SetData(DataPackageView dataPackage)
        {
            SafeWrapper<string> text = await SafeWrapperRoutines.SafeWrapAsync(
                           () => dataPackage.GetTextAsync().AsTask());

            if (!text)
            {
                Debugger.Break();
                return (SafeWrapperResult)text;
            }

            _ContentText = text;

            return (SafeWrapperResult)text;
        }

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

        protected override async Task<SafeWrapper<CollectionItemViewModel>> TrySetFileWithExtension()
        {
            SafeWrapper<CollectionItemViewModel> itemViewModel = await associatedCollection.CreateNewCollectionItemFromExtension(".txt");

            return itemViewModel;
        }

        public override async Task<SafeWrapperResult> TrySaveData()
        {
            SafeWrapperResult result = await FilesystemOperations.WriteFileText(await sourceFile, ContentText);

            return result;
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            OnPropertyChanged(nameof(ContentText));

            return await Task.FromResult(SafeWrapperResult.SUCCESS);
        }

        protected override void RefreshContextMenuItems()
        {
            base.RefreshContextMenuItems();

            // The order is reversed

            // Separator
            ContextMenuItems.AddFront(new MenuFlyoutSeparatorViewModel());

            // Select all
            ContextMenuItems.AddFront(new MenuFlyoutItemViewModel()
            {
                Command = new RelayCommand(() =>
                {
                    ControlView?.TextSelectAll();
                    RefreshContextMenuItems();
                }),
                IconGlyph = "\uE8B3",
                Text = "Select all"
            });

            // TODO: IsTextSelected doesn't seem to work
            if (ControlView != null && ControlView.IsTextSelected)
            {
                // Copy selected text
                ContextMenuItems.AddFront(new MenuFlyoutItemViewModel()
                {
                    Command = new RelayCommand(() => ControlView?.CopySelectedText()),
                    IconGlyph = "\uE8C8",
                    Text = "Copy selected text"
                });
            }
        }

        #endregion

        #region Public Helpers

        public static async Task<bool> CanLoadAsText(StorageFile file)
        {
            // Check if exceeds maximum fileSize or is zero
            long fileSize = await file.GetFileSize();
            if (fileSize > Constants.UI.CanvasContent.FALLBACK_TEXTLOAD_MAX_FILESIZE || fileSize == 0L)
            {
                return false;
            }

            try
            {
                // Check if file is binary
                string text = await FilesystemOperations.ReadFileText(file);
                if (text?.Contains("\0\0\0\0") ?? true)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        #endregion
    }
}
