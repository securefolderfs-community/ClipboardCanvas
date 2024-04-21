using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls;
using ClipboardCanvas.Sdk.ViewModels.Controls.Ribbon;
using ClipboardCanvas.Sdk.ViewModels.Views;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.Shared.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Widgets
{
    public sealed partial class CollectionViewModel : ObservableObject, IEquatable<IDataSourceModel>, IAsyncInitialize
    {
        private readonly IAsyncRelayCommand _navigateBackCommand;
        private readonly IAsyncRelayCommand _navigateForwardCommand;
        private readonly ICollectionSourceModel _collectionStoreModel;
        private readonly NavigationViewModel _navigationViewModel;
        private readonly IDataSourceModel _collectionModel;
        private readonly CanvasViewModel _canvasViewModel;
        private readonly List<IStorableChild> _items;
        private int _index;

        [ObservableProperty] private string? _Name;
        [ObservableProperty] private IImage? _Icon;

        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        private IMediaService ImageService { get; } = Ioc.Default.GetRequiredService<IMediaService>();

        private RibbonViewModel RibbonViewModel { get; } = Ioc.Default.GetRequiredService<RibbonViewModel>();

        public CollectionViewModel(ICollectionSourceModel collectionStoreModel, IDataSourceModel collectionModel, NavigationViewModel navigationViewModel)
        {
            _collectionStoreModel = collectionStoreModel;
            _collectionModel = collectionModel;
            _navigationViewModel = navigationViewModel;
            _navigateBackCommand = new AsyncRelayCommand(GoBackAsync);
            _navigateForwardCommand = new AsyncRelayCommand(GoForwardAsync);
            _canvasViewModel = new(collectionModel, navigationViewModel);
            _items = new();
            Name = collectionModel.Name;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await _collectionModel.InitAsync(cancellationToken);
            Icon = await ImageService.GetCollectionIconAsync(_collectionModel, cancellationToken);
            _items.Clear();
            _items.AddRange(await _collectionModel.GetItemsAsync(StorableType.All, cancellationToken).ToArrayAsync(cancellationToken));
            _index = _items.Count; // Count is out of bounds intentionally to put the index on new (empty) canvas
        }

        /// <inheritdoc/>
        public bool Equals(IDataSourceModel? other)
        {
            return other?.Id == _collectionModel.Id;
        }

        [RelayCommand]
        private async Task OpenCollectionAsync(CancellationToken cancellationToken)
        {
            // TODO: Check if IAsyncInitialize on CanvasViewModel is really needed. It might cause problems
            // where the canvasVM is initialized first in other places
            if (!_navigationViewModel.NavigationService.Views.Contains(_canvasViewModel))
                _ = _canvasViewModel.InitAsync(cancellationToken);

            await _navigationViewModel.NavigationService.NavigateAsync(_canvasViewModel);
            _navigationViewModel.NavigateBackCommand = _navigateBackCommand;
            _navigationViewModel.NavigateForwardCommand = _navigateForwardCommand;
            UpdateNavigationButtons();
        }

        [RelayCommand]
        private Task ShowInFileExplorerAsync(CancellationToken cancellationToken)
        {
            return FileExplorerService.OpenInFileExplorerAsync(_collectionModel, cancellationToken);
        }

        [RelayCommand]
        private Task RenameAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task RemoveAsync(CancellationToken cancellationToken)
        {
            _collectionStoreModel.Remove(_collectionModel);
            await _collectionStoreModel.SaveAsync(cancellationToken);
        }

        private void UpdateNavigationButtons()
        {
            _navigationViewModel.IsForwardEnabled = _index < _items.Count;
            _navigationViewModel.IsBackEnabled = _index > 0 && _items.Count > 0;
        }

        private async Task GoBackAsync(CancellationToken cancellationToken)
        {
            _index -= _index <= 0 ? 0 : 1;
            if (_items.IsEmpty())
                return;

            UpdateNavigationButtons();
            RibbonViewModel.IsRibbonVisible = true;
            var itemToDisplay = _items[_index];
            await _canvasViewModel.DisplayAsync(itemToDisplay, cancellationToken);
        }

        private async Task GoForwardAsync(CancellationToken cancellationToken)
        {
            // Increase by 1, only if not on new canvas
            _index += _index >= _items.Count ? 0 : 1;

            // Update navigation buttons
            UpdateNavigationButtons();

            // If on new canvas...
            if (_index >= _items.Count)
            {
                _canvasViewModel.CurrentCanvasViewModel?.Dispose();
                _canvasViewModel.CurrentCanvasViewModel = null;
                RibbonViewModel.IsRibbonVisible = false;
                return;
            }

            // Otherwise, display the next item...
            var itemToDisplay = _items[_index];
            await _canvasViewModel.DisplayAsync(itemToDisplay, cancellationToken);
        }
    }
}
