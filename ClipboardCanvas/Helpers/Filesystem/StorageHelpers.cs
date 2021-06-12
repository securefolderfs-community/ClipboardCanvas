using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.UnsafeNative;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using ClipboardCanvas.Enums;
using static ClipboardCanvas.UnsafeNative.UnsafeNativeDataModels;

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
            if (path.ToLower().EndsWith(".lnk") || path.ToLower().EndsWith(".url"))
            {
                throw new UnauthorizedAccessException("Function ToStorageItem<TOut>() does not support converting from .lnk nor .url files.");
            }

            // Check if exists
            if (!Exists(path))
            {
                return new SafeWrapper<TOut>(default, OperationErrorCode.NotFound, new System.IO.FileNotFoundException(), "Couldn't resolve item associated with path.");
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
                if (System.IO.Path.HasExtension(path)) // Probably a file
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
                return file as SafeWrapper<TOut>;
            }
            else if (folder != null && folder)
            {
                return folder as SafeWrapper<TOut>;
            }

            return file as SafeWrapper<TOut> ?? (folder as SafeWrapper<TOut> ?? new SafeWrapper<TOut>(default, OperationErrorCode.UnknownFailed, "The operation to get file/folder failed all attempts."));


            async Task GetFile()
            {
                file = await SafeWrapperRoutines.SafeWrapAsync<StorageFile>(() => DangerousStorageFileExtensions.DangerousGetStorageFile(path));
            }

            async Task GetFolder()
            {
                folder = await SafeWrapperRoutines.SafeWrapAsync<StorageFolder>(() => DangerousStorageFileExtensions.DangerousGetStorageFolder(path));
            }
        }

        public static async Task<long> GetFileSize(this IStorageFile file)
        {
            BasicProperties properties = await file.GetBasicPropertiesAsync();
            return (long)properties.Size;
        }

        public static bool Exists(string path)
        {
            return UnsafeNativeApis.GetFileAttributesExFromApp(path, UnsafeNativeDataModels.GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out _);
        }
    }
}
