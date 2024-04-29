using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Ribbon
{
    public partial class ActionViewModel : ObservableObject
    {
        [ObservableProperty] private string? _Name;
        [ObservableProperty] private IImage? _Icon;
        [ObservableProperty] private IAsyncRelayCommand? _Command;
    }
}
