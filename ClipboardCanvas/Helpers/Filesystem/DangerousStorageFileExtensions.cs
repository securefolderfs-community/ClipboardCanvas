using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.Helpers.Filesystem
{
    internal static class DangerousStorageFileExtensions
    {
        internal static async Task<StorageFolder> DangerousGetStorageFolder(string path)
        {
            StorageFolder dangerousFolder = await StorageFolder.GetFolderFromPathAsync(path);

            return dangerousFolder;
        }

        internal static async Task<StorageFile> DangerousGetStorageFile(string path)
        {
            StorageFile dangerousFile = await StorageFile.GetFileFromPathAsync(path);

            return dangerousFile;
        }
    }
}
