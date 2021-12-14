using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.Storage.Base
{
    public abstract class BaseStorageFile : IBaseStorageItem
    {
        public virtual string Name { get; }

        public virtual string Path { get; }

        protected BaseStorageFile(string path)
        {
            this.Path = path;
            this.Name = System.IO.Path.GetFileName(path);
        }

        public abstract Task DeleteAsync(StorageDeleteOption option, IProgress<float> progress, CancellationToken cancellationToken);

        public abstract Task RenameAsync(string desiredName, NameCollisionOption option = NameCollisionOption.GenerateUniqueName);
    }
}
