using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls;
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Widgets
{
    public sealed partial class CollectionViewModel : ObservableObject, ISeekable, IAsyncInitialize, IDisposable
    {
        private readonly ICollectionSourceModel _collectionSourceModel;
        private readonly NavigationViewModel _navigationViewModel;
        private readonly CanvasViewModel _canvasViewModel;
        private readonly List<IStorableChild> _items;
        private int _position;

        [ObservableProperty] private string? _Name;
        [ObservableProperty] private IImage? _Icon;

        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        private IMediaService MediaService { get; } = Ioc.Default.GetRequiredService<IMediaService>();

        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        private RibbonViewModel RibbonViewModel { get; } = Ioc.Default.GetRequiredService<RibbonViewModel>();

        public IDataSourceModel SourceModel { get; }

        public CollectionViewModel(ICollectionSourceModel collectionSourceModel, IDataSourceModel sourceModel, NavigationViewModel navigationViewModel)
        {
            _items = new();
            _collectionSourceModel = collectionSourceModel;
            _navigationViewModel = navigationViewModel;
            _canvasViewModel = new(sourceModel, navigationViewModel, this);
            SourceModel = sourceModel;
            Name = sourceModel.Name;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await SourceModel.InitAsync(cancellationToken);
            SourceModel.CollectionChanged += DataSourceModel_CollectionChanged;
            Icon = await MediaService.GetCollectionIconAsync(SourceModel, cancellationToken);
            _items.Clear();

            // TODO: Order by creation date
            var items = await SourceModel.Source.GetItemsAsync(StorableType.All, cancellationToken).ToArrayAsync(cancellationToken);
            _items.AddRange(items);

            _position = _items.Count; // Count is out of bounds intentionally to put the index on new (empty) canvas
        }

        /// <inheritdoc/>
        public int Seek(int offset, SeekOrigin origin)
        {
            // Return the current position only if not seeking
            if (offset == 0 && origin == SeekOrigin.Current)
                return _position;

            _position = Math.Max(Math.Min(origin switch
            {
                SeekOrigin.End => _items.Count + offset,
                SeekOrigin.Current => _position + offset,
                _ or SeekOrigin.Begin => offset,
            }, _items.Count), 0);

            UpdateNavigationButtons();
            return _position;
        }

        [RelayCommand]
        private async Task OpenCollectionAsync(CancellationToken cancellationToken)
        {
            if (!_navigationViewModel.NavigationService.Views.Contains(_canvasViewModel))
                _ = _canvasViewModel.InitAsync(cancellationToken);

            await _navigationViewModel.NavigationService.NavigateAsync(_canvasViewModel);
            _navigationViewModel.NavigateBackCommand = GoBackCommand;
            _navigationViewModel.NavigateForwardCommand = GoForwardCommand;
            UpdateNavigationButtons();
        }

        [RelayCommand]
        private Task ShowInFileExplorerAsync(CancellationToken cancellationToken)
        {
            return FileExplorerService.OpenInFileExplorerAsync(SourceModel.Source, null, cancellationToken);
        }

        [RelayCommand]
        private Task RenameAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task RemoveAsync(CancellationToken cancellationToken)
        {
            _collectionSourceModel.Remove(SourceModel);
            await _collectionSourceModel.SaveAsync(cancellationToken);
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task GoBackAsync(CancellationToken cancellationToken)
        {
            if (_items.IsEmpty())
                return;

            // Show ribbon if coming from new canvas
            if (_position == _items.Count)
                RibbonViewModel.IsRibbonVisible = true;
            
            Seek(-1, SeekOrigin.Current);
            await NavigateToItemAsync(_items[_position], cancellationToken);
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task GoForwardAsync(CancellationToken cancellationToken)
        {
            Seek(1, SeekOrigin.Current);

            // If on new canvas...
            if (_position >= _items.Count)
            {
                ClearCanvas();
                return;
            }

            // Otherwise, display the next item
            await NavigateToItemAsync(_items[_position], cancellationToken);
        }

        private async Task NavigateToItemAsync(IStorable source, CancellationToken cancellationToken)
        {
            try
            {
                if (SettingsService.UserSettings.UninterruptedResume)
                    SettingsService.AppSettings.LastLocationId = source.Id;

                await _canvasViewModel.DisplayAsync(source, cancellationToken);
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
        }

        private void UpdateNavigationButtons()
        {
            _navigationViewModel.IsForwardEnabled = _position < _items.Count;
            _navigationViewModel.IsBackEnabled = _position > 0 && _items.Count > 0;
        }

        private void ClearCanvas()
        {
            _canvasViewModel.CurrentCanvasViewModel?.Dispose();
            _canvasViewModel.CurrentCanvasViewModel = null;
            RibbonViewModel.IsRibbonVisible = false;
        }

        private async void DataSourceModel_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems is null)
                    return;

                var isNewCanvas = _position == _items.Count;
                foreach (IStorableChild item in e.NewItems)
                    _items.Add(item);

                // TODO: The CurrentCanvasViewModel check might result in race condition
                if (isNewCanvas && _canvasViewModel.CurrentCanvasViewModel is null)
                    _position = _items.Count;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems is null)
                    return;

                var isCanvasInvalid = false;
                var isNewCanvas = _position == _items.Count;
                foreach (IStorable item in e.OldItems)
                {
                    _items.RemoveMatch((x) => x.Id == item.Id);
                    if (!isNewCanvas && _canvasViewModel.CurrentCanvasViewModel?.Storable?.Id == item.Id)
                    {
                        isCanvasInvalid = true;
                        _position = Math.Max(_position - 1, 0);
                    }
                }

                if (isNewCanvas)
                    _position = _items.Count;
                
                if (isCanvasInvalid)
                {
                    var storable = _items.ElementAtOrDefault(_position);
                    if (storable is not null)
                        await _canvasViewModel.DisplayAsync(storable, default);
                    else
                        ClearCanvas();
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            SourceModel.CollectionChanged -= DataSourceModel_CollectionChanged;
            _canvasViewModel.CurrentCanvasViewModel?.Dispose();
        }
    }
}