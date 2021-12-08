using ClipboardCanvas.Storage.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.Storage
{
    public sealed class WindowsStorageFile : BaseStorageFile
    {
        private readonly StorageFile _inner;

        public override string Path => _inner?.Path;

        public override string Name => _inner?.Name;

        private WindowsStorageFile(StorageFile inner)
            : base(null)
        {
            this._inner = inner;
        }

        public static WindowsStorageFile FromWindowsStorageFile(StorageFile storageFile)
        {
            return new WindowsStorageFile(storageFile);
        }

        public override Task DeleteAsync(StorageDeleteOption option, IProgress<float> progress, CancellationToken cancellationToken)
        {
            return _inner.DeleteAsync(option).AsTask();
        }

        public override Task RenameAsync(string desiredName, NameCollisionOption option = NameCollisionOption.GenerateUniqueName)
        {
            return _inner.RenameAsync(desiredName, option).AsTask();
        }
    }
}
