using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Views;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Widgets
{
    public sealed partial class CollectionItemViewModel : ObservableObject, IAsyncInitialize
    {
        private readonly INavigationService _navigationService;
        private readonly ICanvasSourceModel _canvasSourceModel;
        private readonly CanvasViewModel _canvasViewModel;

        [ObservableProperty] private string? _Name;
        [ObservableProperty] private IImage? _Icon;

        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        public CollectionItemViewModel(INavigationService navigationService, ICanvasSourceModel canvasSourceModel)
        {
            _navigationService = navigationService;
            _canvasSourceModel = canvasSourceModel;
            _canvasViewModel = new(canvasSourceModel);
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task OpenCollectionAsync(CancellationToken cancellationToken)
        {
            // Each vm will bind to the same view but will bind to different data (each canvas vm)
            await _navigationService.NavigateAsync(_canvasViewModel);
        }

        [RelayCommand]
        private Task ShowInFileExplorerAsync(CancellationToken cancellationToken)
        {
            return FileExplorerService.OpenInFileExplorerAsync(_canvasSourceModel, cancellationToken);
        }

        [RelayCommand]
        private Task RenameAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private Task RemoveAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
