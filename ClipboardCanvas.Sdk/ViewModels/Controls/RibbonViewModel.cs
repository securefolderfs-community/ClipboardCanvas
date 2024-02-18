using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls
{
    public sealed partial class RibbonViewModel : ObservableObject, IAsyncInitialize
    {
        [ObservableProperty] private string? _FileName;
        [ObservableProperty] private bool _IsRibbonVisible;

        public ObservableCollection<int> PrimaryActions { get; } = new();

        public ObservableCollection<int> SecondaryActions { get; } = new();

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
