using ClipboardCanvas.Shared.ComponentModel;
using OwlCore.Storage.Memory;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ClipboardCanvas.Sdk.Services;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

namespace ClipboardCanvas.Sdk.ViewModels.Widgets
{
    public sealed partial class CollectionsWidgetViewModel : BaseWidgetViewModel, IAsyncInitialize
    {
        private IFileExplorerService FileExplorerService { get; } = Ioc.Default.GetRequiredService<IFileExplorerService>();

        public ObservableCollection<CollectionItemViewModel> Items { get; } = new();

        public CollectionsWidgetViewModel()
        {
            Title = "Collections";
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            Items.Add(new(new MemoryFolder("", "Other")));
            Items.Add(new(new MemoryFolder("", "Vacations")));
            Items.Add(new(new MemoryFolder("", "Work")));
            Items.Add(new(new MemoryFolder("", "Media")));

            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task AddCollectionAsync(CancellationToken cancellationToken)
        {
            var folder = await FileExplorerService.PickFolderAsync(cancellationToken);
            if (folder is null)
                return;

            var collection = new CollectionItemViewModel(folder);
            Items.Add(collection);

            _ = collection.InitAsync(cancellationToken);
        }
    }
}
