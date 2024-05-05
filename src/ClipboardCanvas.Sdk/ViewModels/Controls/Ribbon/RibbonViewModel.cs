using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Ribbon
{
    public sealed partial class RibbonViewModel : ObservableObject
    {
        [ObservableProperty] private string? _RibbonTitle;
        [ObservableProperty] private bool _IsRibbonVisible;
        [ObservableProperty] private ObservableCollection<ActionViewModel>? _PrimaryActions;
        [ObservableProperty] private ObservableCollection<ActionViewModel>? _SecondaryActions;
        [ObservableProperty] private IAsyncRelayCommand<string>? _RenameCommand;
        [ObservableProperty] private IAsyncRelayCommand? _OpenInExplorerCommand;
        [ObservableProperty] private IAsyncRelayCommand? _CopyPathCommand;

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
