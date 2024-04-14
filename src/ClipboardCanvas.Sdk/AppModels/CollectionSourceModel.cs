using ClipboardCanvas.Sdk.DataModels;
using ClipboardCanvas.Sdk.Extensions;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using CommunityToolkit.Mvvm.DependencyInjection;
using OwlCore.Storage;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.AppModels
{
    /// <inheritdoc cref="ICollectionSourceModel"/>
    public sealed class CollectionSourceModel : Collection<IDataSourceModel>, ICollectionSourceModel
    {
        private ICollectionPersistenceService CollectionPersistenceService { get; } = Ioc.Default.GetRequiredService<ICollectionPersistenceService>();

        private IStorageService StorageService { get; } = Ioc.Default.GetRequiredService<IStorageService>();

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            if (CollectionPersistenceService.SavedCollections is not null)
                CollectionPersistenceService.SavedCollections.Clear();

            base.ClearItems();
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, IDataSourceModel item)
        {
            // Update saved collections
            CollectionPersistenceService.SavedCollections ??= new List<CollectionDataModel>();
            CollectionPersistenceService.SavedCollections.Insert(index, new(item.Id, item.Name));

            // Add to cache
            base.InsertItem(index, item);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item, index));
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index)
        {
            var removedItem = this[index];

            // Remove persisted
            if (CollectionPersistenceService.SavedCollections is not null)
                CollectionPersistenceService.SavedCollections.RemoveAt(index);

            // Remove from cache
            base.RemoveItem(index);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, removedItem, index));
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, IDataSourceModel item)
        {
            if (CollectionPersistenceService.SavedCollections is null)
                return;

            CollectionPersistenceService.SavedCollections[index] = new(item.Id, item.Name);

            var oldItem = this[index];
            base.SetItem(index, item);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Replace, item, oldItem, index));
        }

        /// <inheritdoc/>
        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            await CollectionPersistenceService.LoadAsync(cancellationToken);

            // Clear previous collections
            Items.Clear();

            CollectionPersistenceService.SavedCollections ??= new List<CollectionDataModel>();
            foreach (var item in CollectionPersistenceService.SavedCollections)
            {
                if (item.Id is null)
                    continue;

                var folder = await StorageService.TryGetFolderAsync(item.Id, cancellationToken);
                if (folder is not IModifiableFolder modifiableFolder)
                    continue;

                var collectionModel = new CollectionModel(modifiableFolder, item.Name);
                Items.Add(collectionModel);

                CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item));
            }
        }

        /// <inheritdoc/>
        public Task SaveAsync(CancellationToken cancellationToken = default)
        {
            return CollectionPersistenceService.SaveAsync(cancellationToken);
        }
    }
}
