using System.IO;
using System.Threading.Tasks;
using ClipboardCanvas.Enums;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Windows.Storage;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.Dialogs;
using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.UserControls;
using ClipboardCanvas.Services;
using ClipboardCanvas.CanavsPasteModels;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Contexts;
using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.Filesystem;

namespace ClipboardCanvas.Helpers
{
    public static class CanvasHelpers
    {
        public static async Task<SafeWrapperResult> DeleteCanvasFile(ICollectionModel collectionModel, CollectionItemViewModel item, bool hideConfirmation = false)
        {
            bool deletePermanently = false;

            IUserSettingsService userSettings = Ioc.Default.GetService<IUserSettingsService>();
            IDialogService dialogService = Ioc.Default.GetService<IDialogService>();

            if (userSettings.ShowDeleteConfirmationDialog && !hideConfirmation)
            {
                DeleteConfirmationDialogViewModel deleteConfirmationDialogViewModel = new DeleteConfirmationDialogViewModel(Path.GetFileName(item.AssociatedItem.Path));
                DialogResult dialogOption = await dialogService.ShowDialog(deleteConfirmationDialogViewModel);

                if (dialogOption == DialogResult.Primary)
                {
                    deletePermanently = deleteConfirmationDialogViewModel.PermanentlyDelete;
                }
                else
                {
                    return SafeWrapperResult.CANCEL;
                }
            }

            SafeWrapperResult result = await collectionModel.DeleteCollectionItem(item, deletePermanently);

            return result;
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

        public static async Task<SafeWrapperResult> InitializeInfiniteCanvas(CanvasItem infiniteCanvasFolder)
        {
            StorageFolder folder = infiniteCanvasFolder.AssociatedItem as StorageFolder;

            string configurationFileName = Constants.FileSystem.INFINITE_CANVAS_CONFIGURATION_FILENAME;

            return await FilesystemOperations.CreateFile(folder, configurationFileName, CreationCollisionOption.OpenIfExists);
        }
    }
}
