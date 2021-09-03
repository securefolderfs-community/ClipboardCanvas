using System;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.DataModels.ContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.ViewModels.ContextMenu;
using ClipboardCanvas.CanavsPasteModels;
using ClipboardCanvas.Contexts.Operations;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class TextCanvasViewModel : BaseCanvasViewModel
    {
        #region Public Properties

        private TextPasteModel TextPasteModel => canvasPasteModel as TextPasteModel;

        private string _Text;
        public string Text
        {
            get => TextPasteModel?.Text ?? _Text;
        }

        public static List<string> Extensions => new List<string>() {
            ".txt"
        };

        public ITextCanvasControlView ControlView { get; set; }

        #endregion

        #region Constructor

        public TextCanvasViewModel(IBaseCanvasPreviewControlView view, BaseContentTypeModel contentType)
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

            this._Text = text;

            return text;
        }

        protected override Task<SafeWrapperResult> TryFetchDataToView()
        {
            OnPropertyChanged(nameof(Text));

            return Task.FromResult(SafeWrapperResult.SUCCESS);
        }

        protected override IPasteModel SetCanvasPasteModel()
        {
            return new TextPasteModel(AssociatedCollection, new StatusCenterOperationReceiver());
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
                Text = "Select all",
                IsShown = () => (ControlView?.SelectedTextLength ?? 0) < Text.Length
            });

            // Copy selected text
            ContextMenuItems.AddFront(new MenuFlyoutItemViewModel()
            {
                Command = new RelayCommand(() => ControlView?.CopySelectedText()),
                IconGlyph = "\uE8C8",
                Text = "Copy selected text",
                IsShown = () => ControlView?.IsTextSelected ?? false
            });
        }

        #endregion

        #region Helpers

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
