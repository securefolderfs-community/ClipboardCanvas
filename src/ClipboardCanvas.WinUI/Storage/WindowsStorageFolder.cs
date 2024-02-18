using ClipboardCanvas.Sdk.Storage;
using OwlCore.Storage;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.WinUI.Storage
{
    /// <inheritdoc cref="IFolder"/>
    internal sealed class WindowsStorageFolder : WindowsStorable<StorageFolder>, IChildFolder, IDirectCopy, IDirectMove, IFastGetFirstByName
    {
        // TODO: Implement IMutableFolder

        public WindowsStorageFolder(StorageFolder storageFolder)
            : base(storageFolder)
        {
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await storage.GetItemAsync(name).AsTask(cancellationToken) switch
            {
                StorageFile storageFile => new WindowsStorageFile(storageFile),
                StorageFolder storageFolder => new WindowsStorageFolder(storageFolder)
            };
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            switch (type)
            {
                case StorableType.File:
                    {
                        var files = await storage.GetFilesAsync().AsTask(cancellationToken);
                        foreach (var item in files)
                        {
                            yield return new WindowsStorageFile(item);
                        }

                        break;
                    }

                case StorableType.Folder:
                    {
                        var folders = await storage.GetFoldersAsync().AsTask(cancellationToken);
                        foreach (var item in folders)
                        {
                            yield return new WindowsStorageFolder(item);
                        }

                        break;
                    }

                case StorableType.All:
                    {
                        var items = await storage.GetItemsAsync().AsTask(cancellationToken);
                        foreach (var item in items)
                        {
                            if (item is StorageFile storageFile)
                                yield return new WindowsStorageFile(storageFile);

                            if (item is StorageFolder storageFolder)
                                yield return new WindowsStorageFolder(storageFolder);
                        }

                        break;
                    }

                case StorableType.None:
                default:
                    yield break;
            }
        }

        /// <inheritdoc/>
        public Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            return item switch
            {
                WindowsStorable<StorageFile> storageFile => storageFile.storage
                    .DeleteAsync(GetWindowsStorageDeleteOption(false))
                    .AsTask(cancellationToken),

                WindowsStorable<StorageFolder> storageFolder => storageFolder.storage
                    .DeleteAsync(GetWindowsStorageDeleteOption(false))
                    .AsTask(cancellationToken),

                _ => throw new NotImplementedException()
            };
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> CreateCopyOfAsync(IStorableChild itemToCopy, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            if (itemToCopy is WindowsStorable<StorageFile> sourceFile)
            {
                var copiedFile = await sourceFile.storage.CopyAsync(storage, itemToCopy.Name, GetWindowsNameCollisionOption(overwrite)).AsTask(cancellationToken);
                return new WindowsStorageFile(copiedFile);
            }

            throw new ArgumentException($"Could not copy type {itemToCopy.GetType()}");
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> MoveFromAsync(IStorableChild itemToMove, IModifiableFolder source, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            if (itemToMove is WindowsStorable<StorageFile> sourceFile)
            {
                await sourceFile.storage.MoveAsync(storage, itemToMove.Name, GetWindowsNameCollisionOption(overwrite)).AsTask(cancellationToken);
                return new WindowsStorageFile(sourceFile.storage);
            }

            throw new ArgumentException($"Could not copy type {itemToMove.GetType()}");
        }

        /// <inheritdoc/>
        public async Task<IChildFile> CreateFileAsync(string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            var file = await storage.CreateFileAsync(desiredName, GetWindowsCreationCollisionOption(overwrite)).AsTask(cancellationToken);
            return new WindowsStorageFile(file);
        }

        /// <inheritdoc/>
        public async Task<IChildFolder> CreateFolderAsync(string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            var folder = await storage.CreateFolderAsync(desiredName, GetWindowsCreationCollisionOption(overwrite)).AsTask(cancellationToken);
            return new WindowsStorageFolder(folder);
        }

        /// <inheritdoc/>
        public override async Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            var parentFolder = await storage.GetParentAsync().AsTask(cancellationToken);
            return new WindowsStorageFolder(parentFolder);
        }

        /// <inheritdoc/>
        public Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Implement windows watcher
            throw new NotImplementedException();
        }

        private static StorageDeleteOption GetWindowsStorageDeleteOption(bool permanently)
        {
            return permanently ? StorageDeleteOption.PermanentDelete : StorageDeleteOption.Default;
        }

        private static NameCollisionOption GetWindowsNameCollisionOption(bool overwrite)
        {
            return overwrite ? NameCollisionOption.ReplaceExisting : NameCollisionOption.GenerateUniqueName;
        }

        private static CreationCollisionOption GetWindowsCreationCollisionOption(bool overwrite)
        {
            return overwrite ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.OpenIfExists;
        }
    }
}
