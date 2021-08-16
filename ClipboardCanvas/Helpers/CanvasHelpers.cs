using System.IO;
using System.Threading.Tasks;
using ClipboardCanvas.Enums;
using Windows.Storage;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.Dialogs;
using ClipboardCanvas.Services;
using ClipboardCanvas.CanavsPasteModels;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Contexts.Operations;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;

namespace ClipboardCanvas.Helpers
{
    public static class CanvasHelpers
    {
        public static async Task<SafeWrapperResult> DeleteCanvasFile(ICanvasItemReceiverModel canvasItemReceiverModel, CanvasItem canvasItem, bool hideConfirmation = false)
        {
            IUserSettingsService userSettingsService = Ioc.Default.GetService<IUserSettingsService>();

            bool deletePermanently = userSettingsService.DeletePermanentlyAsDefault; // TODO: Option for permanent
            if (userSettingsService.ShowDeleteConfirmationDialog && !hideConfirmation)
            {
                IDialogService dialogService = Ioc.Default.GetService<IDialogService>();

                DeleteConfirmationDialogViewModel deleteConfirmationDialogViewModel = new DeleteConfirmationDialogViewModel(Path.GetFileName(canvasItem.AssociatedItem.Path), deletePermanently);
                DialogResult dialogOption = await dialogService.ShowDialog(deleteConfirmationDialogViewModel);

                if (dialogOption != DialogResult.Primary)
                {
                    return SafeWrapperResult.CANCEL;
                }

                deletePermanently = deleteConfirmationDialogViewModel.PermanentlyDelete;
            }
            else if (hideConfirmation)
            {
                deletePermanently = true;
            }

            // Also remove it from Timeline
            ITimelineService timelineService = Ioc.Default.GetService<ITimelineService>();
            var todaySection = await timelineService.GetOrCreateTodaySection();
            var timelineSectionItem = timelineService.FindTimelineSectionItem(todaySection, canvasItem);
            if (timelineSectionItem != null)
            {
                timelineService.RemoveItemFromSection(todaySection, timelineSectionItem);
            }

            // Delete!
            return await canvasItemReceiverModel.DeleteItem(canvasItem.AssociatedItem, deletePermanently);
        }

        public static CanvasType GetDefaultCanvasType()
        {
            IUserSettingsService userSettings = Ioc.Default.GetService<IUserSettingsService>();

            if (userSettings.UseInfiniteCanvasAsDefault)
            {
                return CanvasType.InfiniteCanvas;
            }
            else
            {
                return CanvasType.OneCanvas;
            }
        }

        public static IPasteModel GetPasteModelFromContentType(this BaseContentTypeModel contentType, ICanvasItemReceiverModel canvasItemReceiverModel, IOperationContextReceiver operationContextReceiver)
        {
            switch (contentType)
            {
                case ImageContentType:
                    return new ImagePasteModel(canvasItemReceiverModel, operationContextReceiver);

                case TextContentType:
                    return new TextPasteModel(canvasItemReceiverModel, operationContextReceiver);

                case MediaContentType:
                    return new MediaPasteModel(canvasItemReceiverModel, operationContextReceiver);

                case MarkdownContentType:
                    return new MarkdownPasteModel(canvasItemReceiverModel, operationContextReceiver);

                case WebViewContentType webViewContentType:
                    return new WebViewPasteModel(webViewContentType.mode, canvasItemReceiverModel, operationContextReceiver);

                case FallbackContentType:
                    return new FallbackPasteModel(canvasItemReceiverModel, operationContextReceiver);

                default:
                    return null;
            }
        }

        public static async Task<SafeWrapper<CanvasItem>> PasteOverrideReference(CanvasItem canvasItem, ICanvasItemReceiverModel canvasItemReceiverModel, IOperationContextReceiver operationContextReceiver)
        {
            if (await canvasItem.SourceItem == null)
            {
                return (null, BaseReadOnlyCanvasViewModel.ReferencedFileNotFoundResult);
            }

            IStorageItem savedSourceItem = await canvasItem.SourceItem;

            // Delete reference file
            SafeWrapperResult deletionResult = await canvasItemReceiverModel.DeleteItem(canvasItem.AssociatedItem, true);
            if (!deletionResult)
            {
                return (null, deletionResult);
            }

            string fileName = (await canvasItem.SourceItem).Name;
            SafeWrapper<CanvasItem> newCanvasItemResult = await canvasItemReceiverModel.CreateNewCanvasFile(fileName);
            if (!newCanvasItemResult)
            {
                return newCanvasItemResult;
            }

            CanvasItem newCanvasItem = newCanvasItemResult.Result;

            // Copy item
            SafeWrapperResult copyResult;
            copyResult = await FilesystemOperations.CopyFileAsync(savedSourceItem as StorageFile, newCanvasItem.AssociatedItem as StorageFile, operationContextReceiver.GetOperationContext());

            if (!copyResult)
            {
                // Failed
                return (null, copyResult);
            }

            return (newCanvasItem, SafeWrapperResult.SUCCESS);
        }
    }
}
