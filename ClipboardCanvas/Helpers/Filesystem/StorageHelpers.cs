using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using System.IO;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Windows.System;

using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.UnsafeNative;
using ClipboardCanvas.ViewModels.UserControls.InAppNotifications;
using ClipboardCanvas.Services;

namespace ClipboardCanvas.Helpers.Filesystem
{
    public static class StorageHelpers
    {
        public static async Task<TOut> ToStorageItem<TOut>(string path) where TOut : IStorageItem
        {
            return (await ToStorageItemWithError<TOut>(path)).Result;
        }

        public static async Task<SafeWrapper<TOut>> ToStorageItemWithError<TOut>(string path) where TOut : IStorageItem
        {
            SafeWrapper<StorageFile> file = null;
            SafeWrapper<StorageFolder> folder = null;

            if (string.IsNullOrWhiteSpace(path))
            {
                return new SafeWrapper<TOut>(default, OperationErrorCode.InvalidArgument, new ArgumentException(), "Provided path is either empty or null.");
            }

            // Check if path is to .lnk or .url file
            if (FilesystemHelpers.IsPathEqualExtension(path, ".lnk") || FilesystemHelpers.IsPathEqualExtension(path, ".url"))
            {
                return new SafeWrapper<TOut>(default, OperationErrorCode.InvalidOperation, new InvalidOperationException(), "Function ToStorageItem<TOut>() does not support converting from .lnk nor .url files.");
            }

            // Check if exists
            if (!Exists(path))
            {
                return new SafeWrapper<TOut>(default, OperationErrorCode.NotFound, new FileNotFoundException(), "Couldn't resolve item associated with path.");
            }

            if (typeof(IStorageFile).IsAssignableFrom(typeof(TOut)))
            {
                await GetFile();
            }
            else if (typeof(IStorageFolder).IsAssignableFrom(typeof(TOut)))
            {
                await GetFolder();
            }
            else if (typeof(IStorageItem).IsAssignableFrom(typeof(TOut)))
            {
                if (Path.HasExtension(path)) // Probably a file
                {
                    await GetFile();
                }
                else // Possibly a folder
                {
                    await GetFolder();

                    if (!folder)
                    {
                        // It wasn't a folder, so check file then because it wasn't checked
                        await GetFile();
                    }
                }
            }

            if (file != null && file)
            {
                return new SafeWrapper<TOut>((TOut)(IStorageItem)file.Result, file);
            }
            else if (folder != null && folder)
            {
                return new SafeWrapper<TOut>((TOut)(IStorageItem)folder.Result, folder);
            }

            return file as SafeWrapper<TOut> ?? (folder as SafeWrapper<TOut> ?? new SafeWrapper<TOut>(default, OperationErrorCode.UnknownFailed, "The operation to get file/folder failed all attempts."));


            async Task GetFile()
            {
                file = await SafeWrapperRoutines.SafeWrapAsync(() => DangerousStorageFileExtensions.DangerousGetStorageFile(path));
            }

            async Task GetFolder()
            {
                folder = await SafeWrapperRoutines.SafeWrapAsync(() => DangerousStorageFileExtensions.DangerousGetStorageFolder(path));
            }
        }

        public static async Task<long> GetFileSize(this IStorageFile file)
        {
            BasicProperties properties = await file.GetBasicPropertiesAsync();
            return (long)properties.Size;
        }

        public static bool Exists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            return UnsafeNativeApis.GetFileAttributesExFromApp(path, UnsafeNativeDataModels.GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out _);
        }

        public static async Task<bool> OpenFile(IStorageItem item)
        {
            if (item == null)
            {
                return false;
            }

            StorageFile file = item as StorageFile;
            string fileExtension = Path.GetExtension(item.Path);
            if (file != null && fileExtension.Equals(".exe", StringComparison.OrdinalIgnoreCase))
            {
                IDialogService dialogService = Ioc.Default.GetService<IDialogService>();

                IInAppNotification notification = dialogService.GetNotification();
                notification.ViewModel.NotificationText = "UWP cannot open executable (.exe) files";
                notification.ViewModel.ShownButtons = InAppNotificationButtonType.OkButton;

                await notification.ShowAsync(4000);
                return false;
            }
            else
            {
                if (file != null)
                {
                    await Launcher.LaunchFileAsync(file);
                    return true;
                }
                else if (item is StorageFolder folder)
                {
                    await Launcher.LaunchFolderAsync(folder);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static async Task<bool> OpenContainingFolder(IStorageItem item)
        {
            if (item == null)
            {
                return false;
            }

            SafeWrapper<StorageFolder> parentFolder;

            string parentFolderPath = Path.GetDirectoryName(item.Path);
            parentFolder = await StorageHelpers.ToStorageItemWithError<StorageFolder>(parentFolderPath);

            if (!parentFolder || parentFolder?.Result == null)
            {
                return false;
            }

            FolderLauncherOptions launcherOptions = new FolderLauncherOptions();
            launcherOptions.ItemsToSelect.Add(item);

            await Launcher.LaunchFolderAsync(parentFolder.Result, launcherOptions);
            return true;
        }
    }
}
