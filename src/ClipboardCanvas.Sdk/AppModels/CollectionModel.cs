using ClipboardCanvas.Sdk.Models;
using OwlCore.Storage;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.AppModels
{
    public sealed class CollectionModel : IDataSourceModel
    {
        private IFolderWatcher? _folderWatcher;

        /// <inheritdoc/>
        public IFolder Source { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public CollectionModel(IFolder folder, string? name = null)
        {
            Source = folder;
            Name = name ?? folder.Name;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (_folderWatcher is not null)
            {
                _folderWatcher.CollectionChanged -= FolderWatcher_CollectionChanged;
                await _folderWatcher.DisposeAsync();
            }

            if (Source is not IMutableFolder mutableFolder)
                return;

            _folderWatcher = await mutableFolder.GetFolderWatcherAsync(cancellationToken);
            _folderWatcher.CollectionChanged += FolderWatcher_CollectionChanged;
        }

        private void FolderWatcher_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
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
