using ClipboardCanvas.Sdk.Extensions;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls.Menu;
using ClipboardCanvas.Sdk.ViewModels.Views.Overlays;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Canvases
{
    public abstract partial class BaseCanvasViewModel : ObservableObject, IAsyncInitialize, IViewable, IDisposable
    {
        [ObservableProperty] private string? _Title;
        [ObservableProperty] private bool _IsEditing;
        [ObservableProperty] private bool _IsImmersed;
        [ObservableProperty] private bool _WasAltered;
        [ObservableProperty] private ObservableCollection<MenuItemViewModel>? _PrimaryActions;
        [ObservableProperty] private ObservableCollection<MenuItemViewModel>? _SecondaryActions;

        public IDataSourceModel SourceModel { get; }

        public virtual IStorable? Storable { get; }

        protected IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        protected IApplicationService ApplicationService { get; } = Ioc.Default.GetRequiredService<IApplicationService>();

        protected IClipboardService ClipboardService { get; } = Ioc.Default.GetRequiredService<IClipboardService>();

        protected IOverlayService OverlayService { get; } = Ioc.Default.GetRequiredService<IOverlayService>();

        protected BaseCanvasViewModel(IStorable storable, IDataSourceModel sourceModel)
            : this(sourceModel)
        {
            Storable = storable;
            Title = Path.GetFileName(storable.Id);
        }

        protected BaseCanvasViewModel(IDataSourceModel sourceModel)
        {
            SourceModel = sourceModel;
        }

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);

        [RelayCommand]
        protected virtual async Task OpenAsync(CancellationToken cancellationToken)
        {
            if (Storable is not IFile file)
                return;

            await ApplicationService.LaunchHandlerAsync(file, cancellationToken);
        }

        [RelayCommand]
        protected virtual async Task ShowInExplorerAsync(CancellationToken cancellationToken)
        {
            await FileExplorerService.OpenInFileExplorerAsync(SourceModel.Source, Storable as IStorableChild, cancellationToken);
        }

        [RelayCommand]
        protected virtual async Task CopyPathAsync(CancellationToken cancellationToken)
        {
            if (Storable is not null)
                await ClipboardService.SetTextAsync(Storable.Id, cancellationToken);
        }

        [RelayCommand]
        protected virtual async Task EditAsync(MenuItemViewModel itemViewModel, CancellationToken cancellationToken)
        {
            if (IsEditing && WasAltered && this is IPersistable persistable)
            {
                if (Storable is not null)
                {
                    var viewModel = new SaveConfirmationOverlayViewModel(Storable.Name);
                    var result = await OverlayService.ShowAsync(viewModel);
                    if (result.Positive())
                    {
                        await persistable.SaveAsync(cancellationToken);
                        IsEditing = false;
                    }
                    else if (result.InBetween())
                    {
                        IsEditing = false;
                    }
                    else
                    {
                        // Aborted, do nothing
                    }
                }
                else
                {
                    // TODO: Display a dialog asking the user if where they want to save the file
                    //var result = await OverlayService.ShowAsync(...);
                }
            }
            else
                IsEditing = !IsEditing;

            // Reset WasAltered when the item is no longer being edited
            if (WasAltered && !IsEditing)
                WasAltered = false;

            if (itemViewModel is MenuToggleViewModel toggleViewModel)
                toggleViewModel.IsToggled = IsEditing;
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }
}
