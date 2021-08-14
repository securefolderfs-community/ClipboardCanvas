using System.IO;
using System.Threading.Tasks;
using ClipboardCanvas.Enums;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.Dialogs;
using ClipboardCanvas.Models;
using ClipboardCanvas.Services;
using ClipboardCanvas.CanavsPasteModels;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Contexts;
using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.ViewModels.UserControls.Collections;

namespace ClipboardCanvas.Helpers
{
    public static class CanvasHelpers
    {
        public static async Task<SafeWrapperResult> DeleteCanvasFile(ICollectionModel collectionModel, CanvasItem canvasItem, bool hideConfirmation = false)
        {
            bool deletePermanently;

            IUserSettingsService userSettings = Ioc.Default.GetService<IUserSettingsService>();
            IDialogService dialogService = Ioc.Default.GetService<IDialogService>();

            if (userSettings.ShowDeleteConfirmationDialog && !hideConfirmation)
            {
                DeleteConfirmationDialogViewModel deleteConfirmationDialogViewModel = new DeleteConfirmationDialogViewModel(Path.GetFileName(canvasItem.AssociatedItem.Path));
                DialogResult dialogOption = await dialogService.ShowDialog(deleteConfirmationDialogViewModel);

                if (dialogOption == DialogResult.Primary)
                {
                    deletePermanently = deleteConfirmationDialogViewModel.PermanentlyDelete;

                    CollectionItemViewModel collectionItem = collectionModel.FindCollectionItem(canvasItem);

                    // Also remove it from Timeline
                    ITimelineService timelineService = Ioc.Default.GetService<ITimelineService>();
                    var todaySection = await timelineService.GetOrCreateTodaySection();
                    var timelineSectionItem = timelineService.FindTimelineSectionItem(todaySection, collectionItem);
                    if (timelineSectionItem != null)
                    {
                        timelineService.RemoveItemFromSection(todaySection, timelineSectionItem);
                    }

                    if (collectionItem == null)
                    {
                        // Just delete the canvasItem
                        return await FilesystemOperations.DeleteItem(canvasItem.AssociatedItem, deletePermanently);
                    }
                    else
                    {
                        // Delete from collection
                        return await collectionModel.DeleteCollectionItem(collectionItem, deletePermanently);
                    }
                }
            }

            return SafeWrapperResult.CANCEL;
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

        public static IPasteModel GetPasteModelFromContentType(this BaseContentTypeModel contentType, ICanvasFileReceiverModel canvasFileReceiver, IOperationContext operationContext)
        {
            switch (contentType)
            {
                case ImageContentType:
                    return new ImagePasteModel(canvasFileReceiver, operationContext);

                case TextContentType:
                    return new TextPasteModel(canvasFileReceiver, operationContext);

                case MediaContentType:
                    return new MediaPasteModel(canvasFileReceiver, operationContext);

                case MarkdownContentType:
                    return new MarkdownPasteModel(canvasFileReceiver, operationContext);

                case FallbackContentType:
                    return new FallbackPasteModel(canvasFileReceiver, operationContext);

                default:
                    return null;
            }
        }
    }
}
