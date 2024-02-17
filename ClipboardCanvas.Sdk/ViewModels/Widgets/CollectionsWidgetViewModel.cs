using ClipboardCanvas.Shared.ComponentModel;
using OwlCore.Storage.Memory;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Shared.Extensions;
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
            //Items.Add(new(new MemoryFolder("", "Other"), null));
            //Items.Add(new(new MemoryFolder("", "Vacations"), null));
            //Items.Add(new(new MemoryFolder("", "Work"), null));
            //Items.Add(new(new MemoryFolder("", "Media"), null));

            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task AddCollectionAsync(CancellationToken cancellationToken)
        {
            var folder = await FileExplorerService.PickFolderAsync(cancellationToken);
            if (folder is null)
                return;

            //Items.Add(new CollectionItemViewModel(folder).WithInitAsync(cancellationToken));
        }
    }
}
