using ClipboardCanvas.Sdk.Services;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace ClipboardCanvas.Sdk.ViewModels.Views.Overlays
{
    public sealed partial class SettingsOverlayViewModel : OverlayViewModel
    {
        public static SettingsOverlayViewModel Instance { get; } = new();

        public INavigationService NavigationService { get; } = Ioc.Default.GetRequiredService<INavigationService>();
    }
}
