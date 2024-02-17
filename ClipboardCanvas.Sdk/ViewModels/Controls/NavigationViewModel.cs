using ClipboardCanvas.Sdk.Extensions;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls
{
    public sealed partial class NavigationViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;

        [ObservableProperty] private bool _IsNavigationVisible;

        public NavigationViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [RelayCommand]
        private Task NavigateBackAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private Task NavigateForwardAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task NavigateHomeAsync(CancellationToken cancellationToken)
        {
            IsNavigationVisible = false;
            await _navigationService.TryNavigateAsync(() => new HomeViewModel());
        }
    }
}
