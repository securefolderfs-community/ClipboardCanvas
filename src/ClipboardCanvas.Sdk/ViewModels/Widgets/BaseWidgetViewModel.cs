using System.Threading;
using System.Threading.Tasks;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardCanvas.Sdk.ViewModels.Widgets
{
    public abstract partial class BaseWidgetViewModel : ObservableObject, IAsyncInitialize, IViewable
    {
        [ObservableProperty] private string? _Title;

        /// <inheritdoc/>
        public abstract Task InitAsync(CancellationToken cancellationToken = default);
    }
}
