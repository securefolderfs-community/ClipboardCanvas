using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.Dialogs;
using ClipboardCanvas.Services;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace ClipboardCanvas.Helpers
{
    public static class InitialApplicationChecksHelpers
    {
        public static async Task CheckVersionAndShowDialog()
        {
            IApplicationSettingsService applicationSettings = Ioc.Default.GetService<IApplicationSettingsService>();
            IDialogService dialogService = Ioc.Default.GetService<IDialogService>();

            string lastVersion = applicationSettings.LastVersionNumber;
            string currentVersion = App.AppVersion;

            if (string.IsNullOrEmpty(lastVersion))
            {
                // No version yet, Clipboard Canvas is freshly installed
                // update the version setting with current version
                applicationSettings.LastVersionNumber = currentVersion;
            }
            else
            {
                // Compare two versions
                if (VersionHelpers.IsVersionDifferentThan(lastVersion, currentVersion))
                {
                    // Update the last version number to be the current number
                    applicationSettings.LastVersionNumber = currentVersion;

                    // Show the update dialog
                    await dialogService.ShowDialog(new UpdateChangeLogDialogViewModel());
                }
            }
        }

        public static async Task HandleFileSystemPermissionDialog(IWindowTitleBarControlModel windowTitleBar)
        {
            IDialogService dialogService = Ioc.Default.GetService<IDialogService>();

            string testForFolderOnFS = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            SafeWrapper<StorageFolder> testFolderResult = await StorageHelpers.ToStorageItemWithError<StorageFolder>(testForFolderOnFS);

            if (!testFolderResult)
            {
                App.IsInRestrictedAccessMode = true;

                DialogResult dialogResult = await dialogService.ShowDialog(new FileSystemAccessDialogViewModel());

                if (dialogResult == DialogResult.Primary)
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
