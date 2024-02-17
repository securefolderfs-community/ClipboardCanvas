using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Widgets
{
    public sealed partial class CollectionItemViewModel : ObservableObject, IAsyncInitialize
    {
        [ObservableProperty] private string? _Name;
        [ObservableProperty] private IImage? _Icon;

        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        public IFolder Folder { get; }

        public CollectionItemViewModel(IFolder folder, string? name = null)
        {
            Folder = folder;
            Name = name ?? folder.Name;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private Task OpenCollectionAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private Task ShowInFileExplorerAsync(CancellationToken cancellationToken)
        {
            return FileExplorerService.OpenInFileExplorerAsync(Folder, cancellationToken);
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
