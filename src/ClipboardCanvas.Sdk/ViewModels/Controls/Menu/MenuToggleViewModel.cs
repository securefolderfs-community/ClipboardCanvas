using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows.Input;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Menu
{
    public partial class MenuToggleViewModel : MenuItemViewModel
    {
        [ObservableProperty] private bool _IsToggled;
        [ObservableProperty] private ICommand? _Command;
    }
}
