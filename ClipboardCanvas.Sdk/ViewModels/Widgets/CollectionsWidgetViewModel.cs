using ClipboardCanvas.Shared.ComponentModel;
using OwlCore.Storage.Memory;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Widgets
{
    public sealed partial class CollectionsWidgetViewModel : BaseWidgetViewModel, IAsyncInitialize
    {
        public ObservableCollection<CollectionItemViewModel> Items { get; } = new();

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            Items.Add(new(new MemoryFolder("", "Other")));
            Items.Add(new(new MemoryFolder("", "Vacations")));
            Items.Add(new(new MemoryFolder("", "Work")));
            Items.Add(new(new MemoryFolder("", "Media")));

            return Task.CompletedTask;
        }
    }
}
