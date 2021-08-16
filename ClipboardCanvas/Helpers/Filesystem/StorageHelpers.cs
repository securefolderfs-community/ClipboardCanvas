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
        public static async Task<TRequested> ToStorageItem<TRequested>(string path) where TRequested : IStorageItem
        {
            return (await ToStorageItemWithError<TRequested>(path)).Result;
        }

        public static async Task<SafeWrapper<TRequested>> ToStorageItemWithError<TRequested>(string path) where TRequested : IStorageItem
        {
            SafeWrapper<StorageFile> file = null;
            SafeWrapper<StorageFolder> folder = null;

            if (string.IsNullOrWhiteSpace(path))
            {
                return new SafeWrapper<TRequested>(default, OperationErrorCode.InvalidArgument, new ArgumentException(), "Provided path is either empty or null.");
            }

            // Check if path is to .lnk or .url file
            if (FilesystemHelpers.IsPathEqualExtension(path, ".lnk") || FilesystemHelpers.IsPathEqualExtension(path, ".url"))
            {
                return new SafeWrapper<TRequested>(default, OperationErrorCode.InvalidOperation, new InvalidOperationException(), "Function ToStorageItem<TOut>() does not support converting from .lnk nor .url files.");
            }

            // Fast get attributes
            bool exists = UnsafeNativeApis.GetFileAttributesExFromApp(path, UnsafeNativeDataModels.GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out UnsafeNativeDataModels.WIN32_FILE_ATTRIBUTE_DATA itemAttributes);
            if (!exists)
            {
                return new SafeWrapper<TRequested>(default, OperationErrorCode.NotFound, new FileNotFoundException(), "Couldn't resolve item associated with path.");
            }

            // Directory
            if (itemAttributes.dwFileAttributes.HasFlag(System.IO.FileAttributes.Directory))
            {
                if (typeof(IStorageFile).IsAssignableFrom(typeof(TRequested))) // Wanted file
                {
                    return new SafeWrapper<TRequested>(default, OperationErrorCode.NotAFile, $"The item at path does not match requested type parameter (TOut: {typeof(TRequested)})");
                }
                else // Just get the directory
                {
                    await GetFolder();
                }
            }
            else // File
            {
                if (typeof(IStorageFolder).IsAssignableFrom(typeof(TRequested))) // Wanted directory
                {
                    return new SafeWrapper<TRequested>(default, OperationErrorCode.NotAFile, $"The item at path does not match requested type parameter (TOut: {typeof(TRequested)})");
                }
                else // Just get the file
                {
                    await GetFile();
                }
            }

            if (file != null && file)
            {
                return new SafeWrapper<TRequested>((TRequested)(IStorageItem)file.Result, file);
            }
            else if (folder != null && folder)
            {
                return new SafeWrapper<TRequested>((TRequested)(IStorageItem)folder.Result, folder);
            }

            return file as SafeWrapper<TRequested> ?? (folder as SafeWrapper<TRequested> ?? new SafeWrapper<TRequested>(default, OperationErrorCode.UnknownFailed, "The operation to get file/folder failed all attempts."));


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

        public static async Task<SafeWrapper<StorageFile>> GetAppSettingsFolder()
        {
            return await FilesystemOperations.CreateFile(ApplicationData.Current.LocalFolder, Constants.LocalSettings.SETTINGS_FOLDERNAME, CreationCollisionOption.OpenIfExists);
        }

        public static async Task<SafeWrapper<StorageFolder>> GetCollectionIconsFolder()
        {
            return await FilesystemOperations.CreateFolder(ApplicationData.Current.LocalFolder, Constants.FileSystem.COLLECTION_ICONS_FOLDERNAME, CreationCollisionOption.OpenIfExists);
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
