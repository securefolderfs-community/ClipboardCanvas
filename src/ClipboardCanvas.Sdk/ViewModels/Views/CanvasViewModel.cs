using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.ViewModels.Controls;
using ClipboardCanvas.Sdk.ViewModels.Controls.Canvases;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Views
{
    public sealed partial class CanvasViewModel : ObservableObject, IViewDesignation, IAsyncInitialize
    {
        private readonly ICanvasSourceModel _canvasSourceModel;
        private readonly NavigationViewModel _navigationViewModel;

        [ObservableProperty] private string? _Title;
        [ObservableProperty] private BaseCanvasViewModel? _CurrentCanvasViewModel;

        public CanvasViewModel(ICanvasSourceModel canvasSourceModel, NavigationViewModel navigationViewModel)
        {
            _canvasSourceModel = canvasSourceModel;
            _navigationViewModel = navigationViewModel;
            Title = canvasSourceModel.Name;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            _ = _canvasSourceModel;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void OnAppearing()
        {
            _navigationViewModel.IsNavigationVisible = true;
        }

        /// <inheritdoc/>
        public void OnDisappearing()
        {
        }

        public async Task DisplayAsync(IStorable source)
        {
            CurrentCanvasViewModel = new TextCanvasViewModel(_canvasSourceModel)
            {
                Text = "Hello World"
            };
        }
    }
}
