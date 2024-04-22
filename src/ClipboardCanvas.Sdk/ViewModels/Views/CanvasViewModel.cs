﻿using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls;
using ClipboardCanvas.Sdk.ViewModels.Controls.Canvases;
using ClipboardCanvas.Sdk.ViewModels.Controls.Ribbon;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;
using ClipboardCanvas.Shared.Enums;

namespace ClipboardCanvas.Sdk.ViewModels.Views
{
    public sealed partial class CanvasViewModel : ObservableObject, IViewDesignation, IAsyncInitialize
    {
        private readonly IDataSourceModel _canvasSourceModel;
        private readonly NavigationViewModel _navigationViewModel;

        [ObservableProperty] private string? _Title;
        [ObservableProperty] private BaseCanvasViewModel? _CurrentCanvasViewModel;

        private ICanvasService CanvasService { get; } = Ioc.Default.GetRequiredService<ICanvasService>();

        private IClipboardService ClipboardService { get; } = Ioc.Default.GetRequiredService<IClipboardService>();

        private RibbonViewModel RibbonViewModel { get; } = Ioc.Default.GetRequiredService<RibbonViewModel>();

        public CanvasViewModel(IDataSourceModel canvasSourceModel, NavigationViewModel navigationViewModel)
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
            RibbonViewModel.IsRibbonVisible = CurrentCanvasViewModel is not null;
            _navigationViewModel.IsNavigationVisible = true;
        }

        /// <inheritdoc/>
        public void OnDisappearing()
        {
            // The canvas is not disposed here since we want the state to be remembered
            // when navigating back to the canvas
        }

        public void ChangeImmersion(bool isImmersed)
        {
            RibbonViewModel.IsRibbonVisible = !isImmersed;
            _navigationViewModel.IsNavigationVisible = !isImmersed;

            if (CurrentCanvasViewModel is not null)
                CurrentCanvasViewModel.IsImmersed = isImmersed;
        }

        public async Task DisplayAsync(IStorable source, CancellationToken cancellationToken)
        {
            // TODO: Infrastructure strategy:
            // For pasting: listen for changes from clipboard (service) and retrieve data friendly format (object, or IData/IFormat)
            // For saving: add a method or use an existing one inside _canvasSourceModel to save items there, however, first check
            // if CurrentCanvasViewModel can be cast to ICanvasSourceModel/IWrapper<ICanvasSourceModel>/InfiniteCanvasViewModel

            if (source is not IStorableChild storable)
                return;

            CurrentCanvasViewModel?.Dispose();
            CurrentCanvasViewModel = await CanvasService.GetCanvasForStorableAsync(storable, _canvasSourceModel, cancellationToken);
        }

        [RelayCommand]
        private async Task PasteFromClipboardAsync(CancellationToken cancellationToken)
        {
            var data = await ClipboardService.GetContentAsync(cancellationToken);
            if (data is null)
                return;

            await (data.Classification.TypeHint switch
            {
                TypeHint.Storage => StorageAsync(),
                TypeHint.Image => ImageAsync(),
                TypeHint.PlainText => TextAsync()
            });

            async Task TextAsync()
            {
                var text = await data.GetTextAsync(cancellationToken);

            }

            async Task ImageAsync()
            {

            }

            async Task StorageAsync()
            {

            }
        }
    }
}
