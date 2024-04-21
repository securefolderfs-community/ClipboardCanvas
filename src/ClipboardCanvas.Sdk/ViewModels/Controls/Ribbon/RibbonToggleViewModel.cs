using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Ribbon
{
    public partial class RibbonToggleViewModel : RibbonItemViewModel
    {
        [ObservableProperty] private bool _IsToggled;
    }
}
