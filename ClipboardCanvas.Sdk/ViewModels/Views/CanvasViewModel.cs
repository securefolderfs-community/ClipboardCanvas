using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.ViewModels.Controls;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Views
{
    public sealed partial class CanvasViewModel : ObservableObject, IViewDesignation, IAsyncInitialize
    {
        private readonly ICanvasSourceModel _canvasSourceModel;
        private readonly NavigationViewModel _navigationViewModel;

        [ObservableProperty] private string? _Title;

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
    }
}
