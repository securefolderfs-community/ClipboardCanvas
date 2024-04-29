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
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Widgets
{
    public sealed partial class CollectionViewModel : ObservableObject, IEquatable<IDataSourceModel>, IAsyncInitialize, IDisposable
    {
        private readonly IAsyncRelayCommand _navigateBackCommand;
        private readonly IAsyncRelayCommand _navigateForwardCommand;
        private readonly ICollectionSourceModel _collectionSourceModel;
        private readonly NavigationViewModel _navigationViewModel;
        private readonly IDataSourceModel _sourceModel;
        private readonly CanvasViewModel _canvasViewModel;
        private readonly List<IStorableChild> _items;
        private int _indexInCollection;

        [ObservableProperty] private string? _Name;
        [ObservableProperty] private IImage? _Icon;

        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        private IMediaService MediaService { get; } = Ioc.Default.GetRequiredService<IMediaService>();

        private RibbonViewModel RibbonViewModel { get; } = Ioc.Default.GetRequiredService<RibbonViewModel>();

        public CollectionViewModel(ICollectionSourceModel collectionSourceModel, IDataSourceModel dataSourceModel, NavigationViewModel navigationViewModel)
        {
            _collectionSourceModel = collectionSourceModel;
            _sourceModel = dataSourceModel;
            _navigationViewModel = navigationViewModel;
            _navigateBackCommand = new AsyncRelayCommand(GoBackAsync);
            _navigateForwardCommand = new AsyncRelayCommand(GoForwardAsync);
            _canvasViewModel = new(dataSourceModel, navigationViewModel);
            dataSourceModel.CollectionChanged += DataSourceModel_CollectionChanged;
            _items = new();
            Name = dataSourceModel.Name;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await _sourceModel.InitAsync(cancellationToken);
            Icon = await MediaService.GetCollectionIconAsync(_sourceModel, cancellationToken);
            _items.Clear();
            _items.AddRange(await _sourceModel.Source.GetItemsAsync(StorableType.All, cancellationToken).ToArrayAsync(cancellationToken));
            _indexInCollection = _items.Count; // Count is out of bounds intentionally to put the index on new (empty) canvas
        }

        /// <inheritdoc/>
        public bool Equals(IDataSourceModel? other)
        {
            return other?.Source.Id == _sourceModel.Source.Id;
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
            return FileExplorerService.OpenInFileExplorerAsync(_sourceModel.Source, cancellationToken);
        }

        [RelayCommand]
        private Task RenameAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task RemoveAsync(CancellationToken cancellationToken)
        {
            _collectionSourceModel.Remove(_sourceModel);
            await _collectionSourceModel.SaveAsync(cancellationToken);
        }

        private async Task GoBackAsync(CancellationToken cancellationToken)
        {
            _indexInCollection -= _indexInCollection <= 0 ? 0 : 1;
            if (_items.IsEmpty())
                return;

            UpdateNavigationButtons();
            RibbonViewModel.IsRibbonVisible = true;
            var itemToDisplay = _items[_indexInCollection];
            await _canvasViewModel.DisplayAsync(itemToDisplay, cancellationToken);
        }

        private async Task GoForwardAsync(CancellationToken cancellationToken)
        {
            // Increase by 1, only if not on new canvas
            _indexInCollection += _indexInCollection >= _items.Count ? 0 : 1;

            // Update navigation buttons
            UpdateNavigationButtons();

            // If on new canvas...
            if (_indexInCollection >= _items.Count)
            {
                _canvasViewModel.CurrentCanvasViewModel?.Dispose();
                _canvasViewModel.CurrentCanvasViewModel = null;
                RibbonViewModel.IsRibbonVisible = false;
                return;
            }

            // Otherwise, display the next item...
            var itemToDisplay = _items[_indexInCollection];
            await _canvasViewModel.DisplayAsync(itemToDisplay, cancellationToken);
        }

        private void UpdateNavigationButtons()
        {
            _navigationViewModel.IsForwardEnabled = _indexInCollection < _items.Count;
            _navigationViewModel.IsBackEnabled = _indexInCollection > 0 && _items.Count > 0;
        }

        private void DataSourceModel_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _ = e;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _sourceModel.Dispose();
        }
    }
}
