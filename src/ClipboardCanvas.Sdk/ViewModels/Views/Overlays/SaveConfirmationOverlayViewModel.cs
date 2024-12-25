using CommunityToolkit.Mvvm.ComponentModel;

namespace ClipboardCanvas.Sdk.ViewModels.Views.Overlays
{
    public sealed partial class SaveConfirmationOverlayViewModel : OverlayViewModel
    {
        [ObservableProperty] private string? _FileName;

        public SaveConfirmationOverlayViewModel(string fileName)
        {
            FileName = fileName;
        }
    }
}
