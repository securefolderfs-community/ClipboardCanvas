using ClipboardCanvas.Sdk.Helpers;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls;
using ClipboardCanvas.Sdk.ViewModels.Controls.Canvases;
using ClipboardCanvas.Sdk.ViewModels.Controls.Menu;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.Shared.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Views
{
    public sealed partial class CanvasViewModel : ObservableObject, IViewDesignation, IAsyncInitialize
    {
        private readonly IDataSourceModel _canvasSourceModel;
        private readonly ISeekable? _seekable;

        [ObservableProperty] private string? _Title;
        [ObservableProperty] private NavigationViewModel _NavigationViewModel;
        [ObservableProperty] private BaseCanvasViewModel? _CurrentCanvasViewModel;

        private ICanvasService CanvasService { get; } = Ioc.Default.GetRequiredService<ICanvasService>();

        private IClipboardService ClipboardService { get; } = Ioc.Default.GetRequiredService<IClipboardService>();

        private QuickOptionsViewModel QuickOptionsViewModel { get; } = Ioc.Default.GetRequiredService<QuickOptionsViewModel>();

        private RibbonViewModel RibbonViewModel { get; } = Ioc.Default.GetRequiredService<RibbonViewModel>();

        public CanvasViewModel(IDataSourceModel canvasSourceModel, NavigationViewModel navigationViewModel, ISeekable? seekable)
        {
            _canvasSourceModel = canvasSourceModel;
            _seekable = seekable;
            NavigationViewModel = navigationViewModel;
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
            QuickOptionsViewModel.CreateNewDocumentCommand = CreateNewDocumentCommand;
            RibbonViewModel.IsRibbonVisible = CurrentCanvasViewModel is not null;
            NavigationViewModel.IsNavigationVisible = true;
        }

        /// <inheritdoc/>
        public void OnDisappearing()
        {
            // The canvas is not disposed here since we want the state
            // to be remembered when navigating back to the canvas
        }

        public void ChangeImmersion(bool isImmersed)
        {
            RibbonViewModel.IsRibbonVisible = !isImmersed;
            NavigationViewModel.IsNavigationVisible = !isImmersed;

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

            var canvasViewModel = await CanvasService.GetCanvasForStorableAsync(storable, _canvasSourceModel, cancellationToken);
            CurrentCanvasViewModel?.Dispose();
            CurrentCanvasViewModel = canvasViewModel;

            BindRibbon();
        }

        [Obsolete("This method will be replaced in the future")]
        private void BindRibbon()
        {
            if (CurrentCanvasViewModel is null)
                return;

            // TODO: Find a better way to bind
            RibbonViewModel.RibbonTitle = CurrentCanvasViewModel.Title;
            RibbonViewModel.PrimaryActions = CurrentCanvasViewModel.PrimaryActions;
            RibbonViewModel.SecondaryActions = CurrentCanvasViewModel.SecondaryActions;
            RibbonViewModel.CopyPathCommand = CurrentCanvasViewModel.CopyPathCommand;
            RibbonViewModel.ShowInExplorerCommand = CurrentCanvasViewModel.ShowInExplorerCommand;
        }

        [RelayCommand]
        private async Task PasteFromClipboardAsync(CancellationToken cancellationToken)
        {
            var data = await ClipboardService.GetContentAsync(cancellationToken);
            if (data is null)
                return;

            var sourceModel = CurrentCanvasViewModel as IDataSourceModel ?? _canvasSourceModel;
            var canvasViewModel = data.Classification.TypeHint switch
            {
                TypeHint.Image => await ImageCanvasViewModel.ParseAsync(data, sourceModel, cancellationToken),
                TypeHint.PlainText => await TextCanvasViewModel.ParseAsync(data, sourceModel, cancellationToken),
                TypeHint.Storage => await StorageAsync(),

                _ or TypeHint.Unclassified => null
            };

            CurrentCanvasViewModel?.Dispose();
            CurrentCanvasViewModel = canvasViewModel;
            BindRibbon();

            Task<BaseCanvasViewModel> StorageAsync()
            {
                // TODO: Maybe use infinite canvas or only allow for one item to be pasted?
                // Perhaps prompt the user to choose what to do?

                throw new NotImplementedException();
            }
        }

        [RelayCommand]
        private async Task CreateNewDocumentAsync(CancellationToken cancellationToken)
        {
            if (_canvasSourceModel.Source is not IModifiableFolder modifiableFolder)
                return;

            // Seek to new canvas
            var position = _seekable?.Seek(0, SeekOrigin.End) ?? 0;

            // Create new file
            var fileName = FormattingHelpers.GetDateFileName(".txt");
            var file = await modifiableFolder.CreateFileAsync(fileName, false, cancellationToken);

            // Display the text canvas
            await DisplayAsync(file, cancellationToken);
            RibbonViewModel.IsRibbonVisible = true;

            // Immediately seek back to synchronize the position after the item as successfully added to the collection.
            // This is done because when a new item is added and the index is on new canvas, the position will update
            // to stay on the new canvas.
            _seekable?.Seek(position, SeekOrigin.Begin);

            // TODO: This is a workaround since we know that the only toggle is the Edit one
            // A better approach would be to change the property directly in the command itself

            // Start editing
            var item = CurrentCanvasViewModel?.PrimaryActions?.FirstOrDefault(static x => x is MenuToggleViewModel);
            if (CurrentCanvasViewModel is not null)
                await CurrentCanvasViewModel.EditCommand.ExecuteAsync(item);
        }
    }
}
