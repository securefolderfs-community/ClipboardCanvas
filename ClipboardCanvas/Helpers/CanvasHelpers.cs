using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using ClipboardCanvas.Enums;

using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.Dialogs;

namespace ClipboardCanvas.Helpers
{
    public static class CanvasHelpers
    {
        public static async Task<SafeWrapperResult> DeleteCanvasFile(StorageFile file, bool hideConfirmation = false)
        {
            bool deletePermanently = false;

            if (App.AppSettings.UserSettings.ShowDeleteConfirmationDialog && !hideConfirmation)
            {
                DeleteConfirmationDialogViewModel deleteConfirmationDialogViewModel = new DeleteConfirmationDialogViewModel(Path.GetFileName(file.Path));
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

            SafeWrapperResult result = await FilesystemOperations.DeleteItem(file, deletePermanently);

            return result;
        }
    }
}
