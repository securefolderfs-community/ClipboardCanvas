using ClipboardCanvas.Sdk.ViewModels.Controls.Menu;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClipboardCanvas.Sdk.ViewModels.Controls
{
    public sealed partial class RibbonViewModel : ObservableObject
    {
        [ObservableProperty] private string? _RibbonTitle;
        [ObservableProperty] private bool _IsRibbonVisible;
        [ObservableProperty] private ObservableCollection<MenuItemViewModel>? _PrimaryActions;
        [ObservableProperty] private ObservableCollection<MenuItemViewModel>? _SecondaryActions;
        [ObservableProperty] private ICommand? _RenameCommand;
        [ObservableProperty] private ICommand? _ShowInExplorerCommand;
        [ObservableProperty] private ICommand? _CopyPathCommand;

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
