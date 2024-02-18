using ClipboardCanvas.Sdk.AppModels;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.Shared.Extensions;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Widgets
{
    public sealed partial class CollectionsWidgetViewModel : BaseWidgetViewModel, IAsyncInitialize
    {
        private readonly ICollectionStoreModel _collectionStoreModel;
        private readonly NavigationViewModel _navigationViewModel;

        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        public ObservableCollection<CollectionItemViewModel> Items { get; } = new();

        public CollectionsWidgetViewModel(ICollectionStoreModel collectionStoreModel, NavigationViewModel navigationViewModel)
        {
            _collectionStoreModel = collectionStoreModel;
            _navigationViewModel = navigationViewModel;
            Title = "Collections";

            _collectionStoreModel.CollectionChanged += CollectionStoreModel_CollectionChanged;
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            Items.Clear();
            foreach (var item in _collectionStoreModel)
                Items.Add(new(_collectionStoreModel, item, _navigationViewModel));

            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task AddCollectionAsync(CancellationToken cancellationToken)
        {
            var folder = await FileExplorerService.PickFolderAsync(cancellationToken);
            if (folder is not IModifiableFolder modifiableFolder)
                return;

            _collectionStoreModel.Add(new CollectionModel(modifiableFolder));
            await _collectionStoreModel.SaveAsync(cancellationToken);
        }

        private void CollectionStoreModel_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add when e.NewItems is not null && e.NewItems[0] is ICanvasSourceModel collectionModel:
                    Items.Add(new(_collectionStoreModel, collectionModel, _navigationViewModel));
                    break;

                case NotifyCollectionChangedAction.Remove when e.OldItems is not null && e.OldItems[0] is ICanvasSourceModel collectionModel:
                    Items.RemoveMatch(x => x.Equals(collectionModel));
                    break;

                case NotifyCollectionChangedAction.Reset:
                    Items.Clear();
                    break;
            }
        }
    }
}
