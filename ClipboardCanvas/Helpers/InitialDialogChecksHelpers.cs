using ClipboardCanvas.Dialogs;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace ClipboardCanvas.Helpers
{
    public static class InitialDialogChecksHelpers
    {
        public static async Task CheckVersionAndShowDialog()
        {
            string lastVersion = App.AppSettings.ApplicationSettings.LastVersionNumber;
            string currentVersion = App.AppVersion;

            if (string.IsNullOrEmpty(lastVersion))
            {
                // No version yet, Clipboard Canvas is freshly installed
                // update the version setting with current version
                App.AppSettings.ApplicationSettings.LastVersionNumber = currentVersion;
            }
            else
            {
                return;

                // Compare two versions
                if (ApplicationHelpers.IsVersionDifferentThan(lastVersion, currentVersion))
                {
                    UpdateChangeLogDialog updateChangeLogDialog = new UpdateChangeLogDialog();
                    
                    // Show the update dialog
                    await updateChangeLogDialog.ShowAsync();

                    // Update the last version number to be the current number
                    App.AppSettings.ApplicationSettings.LastVersionNumber = currentVersion;
                }
            }
        }

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
