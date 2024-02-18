using ClipboardCanvas.Sdk.Extensions;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.Shared.Extensions;
using CommunityToolkit.Mvvm.DependencyInjection;
using OwlCore.Storage;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.AppModels
{
    public sealed class CollectionModel : ICanvasSourceModel, IWrapper<IModifiableFolder>
    {
        private readonly List<IStorableChild> _cachedCanvases;
        private IFolderWatcher? _folderWatcher;

        private IStorageService StorageService { get; } = Ioc.Default.GetRequiredService<IStorageService>();

        /// <inheritdoc/>
        public IModifiableFolder Inner { get; }

        /// <inheritdoc/>
        public string Id => Inner.Id;

        /// <inheritdoc/>
        public string Name => Inner.Name;

        public CollectionModel(IModifiableFolder folder)
        {
            Inner = folder;
            _cachedCanvases = new();
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            _cachedCanvases.Clear();
            await foreach (var item in Inner.GetItemsAsync(StorableType.All, cancellationToken))
            {
                _cachedCanvases.Add(item);
            }

            if (_folderWatcher is not null)
            {
                _folderWatcher.CollectionChanged -= FolderWatcher_CollectionChanged;
                await _folderWatcher.DisposeAsync();
            }

            _folderWatcher = await Inner.GetFolderWatcherAsync(cancellationToken);
            _folderWatcher.CollectionChanged += FolderWatcher_CollectionChanged;
        }

        private async void FolderWatcher_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e is { Action: NotifyCollectionChangedAction.Add, NewItems: not null })
            {
                foreach (IStorable item in e.NewItems)
                {
                    // Need to convert the simple storable object into a fully-fledged implementation
                    var storable = await StorageService.TryGetStorableAsync(item.Id);
                    if (storable is null)
                        continue;

                    _cachedCanvases.Add(storable);
                }
            }
            else if (e is { Action: NotifyCollectionChangedAction.Remove, OldItems: not null })
            {
                foreach (IStorable item in e.OldItems)
                {
                    _cachedCanvases.RemoveMatch(x => x.Id == item.Id);
                }
            }
            // TODO: Implement rename
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (type == StorableType.None)
                yield break;

            foreach (var item in _cachedCanvases)
            {
                var isFile = item is IFile;
                if (type == StorableType.Folder && isFile)
                    continue;

                yield return item;
            }

            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = default)
        {
            return Inner.GetFolderWatcherAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            await Inner.DeleteAsync(item, cancellationToken);
            _cachedCanvases.Remove(item);
        }

        /// <inheritdoc/>
        public async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var folder = await Inner.CreateFolderAsync(name, overwrite, cancellationToken);
            _cachedCanvases.Add(folder);

            return folder;
        }

        /// <inheritdoc/>
        public async Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var file = await Inner.CreateFileAsync(name, overwrite, cancellationToken);
            _cachedCanvases.Add(file);

            return file;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (_folderWatcher is not null)
            {
                _folderWatcher.CollectionChanged -= FolderWatcher_CollectionChanged;
                await _folderWatcher.DisposeAsync();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_folderWatcher is not null)
            {
                _folderWatcher.CollectionChanged -= FolderWatcher_CollectionChanged;
                _folderWatcher.Dispose();
            }
        }
    }
}
