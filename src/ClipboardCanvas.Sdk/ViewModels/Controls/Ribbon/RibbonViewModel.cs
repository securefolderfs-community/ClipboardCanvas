using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Ribbon
{
    public sealed partial class RibbonViewModel : ObservableObject, IAsyncInitialize
    {
        [ObservableProperty] private string? _RibbonTitle;
        [ObservableProperty] private bool _IsRibbonVisible;
        [ObservableProperty] private ObservableCollection<RibbonItemViewModel>? _PrimaryActions;
        [ObservableProperty] private ObservableCollection<RibbonItemViewModel>? _SecondaryActions;

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
