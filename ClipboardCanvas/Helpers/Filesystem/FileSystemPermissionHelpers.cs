using ClipboardCanvas.Dialogs;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace ClipboardCanvas.Helpers.Filesystem
{
    public static class FileSystemPermissionHelpers
    {
        public static async Task HandleFileSystemPermissionDialog(IWindowTitleBarControlModel windowTitleBar)
        {
            string testForFolderOnFS = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            SafeWrapper<StorageFolder> testFolderResult = await StorageItemHelpers.ToStorageItemWithError<StorageFolder>(testForFolderOnFS);

            if (!testFolderResult)
            {
                App.IsInRestrictedAccessMode = true;

                FileSystemAccessDialog fileSystemAccessDialog = new FileSystemAccessDialog();
                ContentDialogResult dialogResult = await fileSystemAccessDialog.ShowAsync();

                if (dialogResult == ContentDialogResult.Primary)
                {
                    // Restart the app
                    await CoreApplication.RequestRestartAsync(string.Empty);
                }
                else
                {
                    // Continue in Restricted Access
                    windowTitleBar.IsInRestrictedAccess = true;
                }
            }
            else
            {
                return;
            }
        }
    }
}
