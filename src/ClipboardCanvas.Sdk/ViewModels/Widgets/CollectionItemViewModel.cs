using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls;
using ClipboardCanvas.Sdk.ViewModels.Views;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Widgets
{
    public sealed partial class CollectionItemViewModel : ObservableObject, IEquatable<ICanvasSourceModel>, IAsyncInitialize
    {
        private readonly ICollectionStoreModel _collectionStoreModel;
        private readonly NavigationViewModel _navigationViewModel;
        private readonly ICanvasSourceModel _collectionModel;
        private readonly CanvasViewModel _canvasViewModel;

        [ObservableProperty] private string? _Name;
        [ObservableProperty] private IImage? _Icon;

        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        private IImageService ImageService { get; } = Ioc.Default.GetRequiredService<IImageService>();

        public CollectionItemViewModel(ICollectionStoreModel collectionStoreModel, ICanvasSourceModel collectionModel, NavigationViewModel navigationViewModel)
        {
            _collectionStoreModel = collectionStoreModel;
            _collectionModel = collectionModel;
            _navigationViewModel = navigationViewModel;
            _canvasViewModel = new(collectionModel, navigationViewModel);
            Name = collectionModel.Name;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await _collectionModel.InitAsync(cancellationToken);
            Icon = await ImageService.GetCollectionIconAsync(_collectionModel, cancellationToken);
        }

        /// <inheritdoc/>
        public bool Equals(ICanvasSourceModel? other)
        {
            return other?.Id == _collectionModel.Id;
        }

        [RelayCommand]
        private async Task OpenCollectionAsync(CancellationToken cancellationToken)
        {
            // Each vm will bind to the same view but will bind to different data (each canvas vm)
            await _navigationViewModel.NavigationService.NavigateAsync(_canvasViewModel);
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
    }
}
