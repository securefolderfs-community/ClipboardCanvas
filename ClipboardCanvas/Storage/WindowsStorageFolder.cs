using ClipboardCanvas.Storage.Base;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using System;

namespace ClipboardCanvas.Storage
{
    public sealed class WindowsStorageFolder : BaseStorageFolder
    {
        private readonly StorageFolder _inner;

        private WindowsStorageFolder(StorageFolder inner)
            : base(null)
        {
            this._inner = inner;
        }

        public static WindowsStorageFolder FromWindowsStorageFile(StorageFolder storageFolder)
        {
            return new WindowsStorageFolder(storageFolder);
        }

        public override Task DeleteAsync(StorageDeleteOption option, IProgress<float> progress, CancellationToken cancellationToken)
        {
            return _inner.DeleteAsync(option).AsTask();
        }

        public override Task DeleteAsync(StorageDeleteOption option, bool recursive, IProgress<float> progress, CancellationToken cancellationToken)
        {
            return _inner.DeleteAsync(option).AsTask();
        }

        public override Task RenameAsync(string desiredName, NameCollisionOption option = NameCollisionOption.GenerateUniqueName)
        {
            return _inner.RenameAsync(desiredName, option).AsTask();
        }
    }
}
