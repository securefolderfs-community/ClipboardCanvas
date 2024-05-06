using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls.Ribbon;
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
        [ObservableProperty] private ObservableCollection<ActionViewModel>? _PrimaryActions;
        [ObservableProperty] private ObservableCollection<ActionViewModel>? _SecondaryActions;

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
        protected virtual Task EditAsync(CancellationToken cancellationToken)
        {
            if (IsEditing && WasAltered && this is IPersistable persistable)
            {
                // TODO: Display a dialog asking the user if they want to save or discard the changes
                //var result = await OverlayService.ShowAsync(...);
            }
            else
                IsEditing = !IsEditing;

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }
}
