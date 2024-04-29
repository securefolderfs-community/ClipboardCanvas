using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Ribbon
{
    public partial class ToggleViewModel : ActionViewModel
    {
        [ObservableProperty] private bool _IsToggled;
    }
}
