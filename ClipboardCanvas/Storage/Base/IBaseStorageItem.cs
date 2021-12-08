using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.Storage.Base
{
    public interface IBaseStorageItem
    {
        string Name { get; }

        string Path { get; }

        Task RenameAsync(string desiredName, NameCollisionOption option = NameCollisionOption.GenerateUniqueName);

        Task DeleteAsync(StorageDeleteOption option, IProgress<float> progress, CancellationToken cancellationToken);
    }
}
