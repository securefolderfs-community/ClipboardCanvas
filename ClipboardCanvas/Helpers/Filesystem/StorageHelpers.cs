using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using System.IO;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Windows.System;
using Microsoft.Win32.SafeHandles;

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
            if (FileHelpers.IsPathEqualExtension(path, ".lnk") || FileHelpers.IsPathEqualExtension(path, ".url"))
            {
                return new SafeWrapper<TRequested>(default, OperationErrorCode.InvalidOperation, new InvalidOperationException(), "Function ToStorageItem<TRequested>() does not support converting from .lnk nor .url files.");
            }

            // Fast get attributes
            bool exists = UnsafeNativeApis.GetFileAttributesExFromApp(path, UnsafeNativeDataModels.GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out UnsafeNativeDataModels.WIN32_FILE_ATTRIBUTE_DATA itemAttributes);
            if (!exists)
            {
                uint hresult = UnsafeNativeApis.GetLastError();
                return new SafeWrapper<TRequested>(default, OperationErrorCode.NotFound, new FileNotFoundException($"The file was not found. Actual HRESULT:{hresult}"), "Couldn't resolve item associated with path.");
            }

            // Directory
            if (itemAttributes.dwFileAttributes.HasFlag(System.IO.FileAttributes.Directory))
            {
                if (typeof(IStorageFile).IsAssignableFrom(typeof(TRequested))) // Wanted file
                {
                    return new SafeWrapper<TRequested>(default, OperationErrorCode.NotAFile, $"The item at path does not match requested type parameter (TRequested: {typeof(TRequested)})");
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
                    return new SafeWrapper<TRequested>(default, OperationErrorCode.NotAFile, $"The item at path does not match requested type parameter (TRequested: {typeof(TRequested)})");
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

        public static FileStream OpenFileStreamh(this IStorageFile file, FileAccess fileAccess = FileAccess.ReadWrite)
        {
            SafeFileHandle hFile = file.CreateSafeFileHandle(fileAccess);
            FileStream fileStream = new FileStream(hFile, fileAccess);

            return fileStream;
        }

        public static async Task<long> GetFileSize(this IStorageFile file)
        {
            if (file == null)
            {
                return -1L;
            }

            BasicProperties properties = await file.GetBasicPropertiesAsync();
            return (long)properties.Size;
        }

        public static bool Existsh(string path)
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

        public static async Task<SafeWrapper<StorageFile>> GetExceptionLogFile()
        {
            string exceptionLogFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.FileSystem.EXCEPTIONLOG_FILENAME);

            return await ToStorageItemWithError<StorageFile>(exceptionLogFilePath);
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

                notification.Show(Constants.UI.Notifications.NOTIFICATION_DEFAULT_SHOW_TIME);
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
