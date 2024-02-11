using ClipboardCanvas.Sdk.ViewModels.Widgets;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Views
{
    public sealed partial class HomeViewModel : ObservableObject, IAsyncInitialize
    {
        public ObservableCollection<BaseWidgetViewModel> Widgets { get; } = new();

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var widget = new CollectionsWidgetViewModel();
            Widgets.Add(widget);

            await widget.InitAsync(cancellationToken);
        }
    }
}
