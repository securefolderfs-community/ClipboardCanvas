using System.IO;
using System.Threading.Tasks;
using ClipboardCanvas.Enums;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.Dialogs;
using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.UserControls;

namespace ClipboardCanvas.Helpers
{
    public static class CanvasHelpers
    {
        public static async Task<SafeWrapperResult> DeleteCanvasFile(ICollectionModel collectionModel, CollectionItemViewModel item, bool hideConfirmation = false)
        {
            bool deletePermanently = false;

            if (App.AppSettings.UserSettings.ShowDeleteConfirmationDialog && !hideConfirmation)
            {
                DeleteConfirmationDialogViewModel deleteConfirmationDialogViewModel = new DeleteConfirmationDialogViewModel(Path.GetFileName(item.AssociatedItem.Path));
                DialogResult dialogOption = await App.DialogService.ShowDialog(deleteConfirmationDialogViewModel);

                if (dialogOption == DialogResult.Primary)
                {
                    deletePermanently = deleteConfirmationDialogViewModel.PermanentlyDelete;
                }
                else
                {
                    return SafeWrapperResult.S_CANCEL;
                }
            }

            SafeWrapperResult result = await collectionModel.DeleteCollectionItem(item, deletePermanently);

            return result;
        }

        public static CanvasType GetDefaultCanvasType()
        {
            if (App.AppSettings.UserSettings.UseInfiniteCanvasAsDefault)
            {
                return CanvasType.InfiniteCanvas;
            }
            else
            {
                return CanvasType.NormalCanvas;
            }
        }
    }
}
