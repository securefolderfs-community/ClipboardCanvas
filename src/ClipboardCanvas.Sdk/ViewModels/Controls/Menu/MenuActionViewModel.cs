using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Menu
{
    public partial class MenuActionViewModel : MenuItemViewModel
    {
        [ObservableProperty] private ICommand? _Command;
    }
}
