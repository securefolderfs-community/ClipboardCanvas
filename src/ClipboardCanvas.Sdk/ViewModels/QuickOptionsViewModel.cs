using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Views.Overlays;
using ClipboardCanvas.Shared.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClipboardCanvas.Sdk.ViewModels
{
    public sealed partial class QuickOptionsViewModel : ObservableObject
    {
        [ObservableProperty] private ICommand? _CreateNewDocumentCommand;

        private IOverlayService OverlayService { get; } = Ioc.Default.GetRequiredService<IOverlayService>();

        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        [RelayCommand]
        private async Task OpenSettingsAsync()
        {
            await OverlayService.ShowAsync(SettingsOverlayViewModel.Instance);
            await SettingsService.TrySaveAsync();
        }
    }
}
