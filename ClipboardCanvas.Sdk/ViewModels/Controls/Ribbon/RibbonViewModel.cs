using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Ribbon
{
    public sealed partial class RibbonViewModel : ObservableObject, IAsyncInitialize
    {
        [ObservableProperty] private string? _FileName;
        [ObservableProperty] private bool _IsRibbonVisible;

        public ObservableCollection<RibbonActionViewModel> PrimaryActions { get; } = new();

        public ObservableCollection<RibbonActionViewModel> SecondaryActions { get; } = new();

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
