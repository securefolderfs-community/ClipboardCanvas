using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Ribbon
{
    public sealed partial class RibbonActionViewModel : ObservableObject
    {
        [ObservableProperty] private string? _Name;
        [ObservableProperty] private IImage? _Icon;
        [ObservableProperty] private IAsyncRelayCommand _Command;

        public RibbonActionViewModel(string? name, IImage? icon, IAsyncRelayCommand command)
        {
            _Name = name;
            _Icon = icon;
            _Command = command;
        }
    }
}
