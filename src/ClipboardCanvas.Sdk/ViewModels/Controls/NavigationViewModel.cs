using ClipboardCanvas.Sdk.Extensions;
using ClipboardCanvas.Sdk.Models;
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
        private readonly ICollectionStoreModel _collectionStoreModel;

        [ObservableProperty] private IAsyncRelayCommand? _NavigateBackCommand;
        [ObservableProperty] private IAsyncRelayCommand? _NavigateForwardCommand;
        [ObservableProperty] private bool _IsNavigationVisible;
        [ObservableProperty] private bool _IsForwardEnabled;
        [ObservableProperty] private bool _IsBackEnabled;

        public INavigationService NavigationService { get; }

        public NavigationViewModel(ICollectionStoreModel collectionStoreModel, INavigationService navigationService)
        {
            _collectionStoreModel = collectionStoreModel;
            NavigationService = navigationService;
            IsNavigationVisible = true;
            IsForwardEnabled = true;
            IsBackEnabled = true;
        }

        [RelayCommand]
        private async Task NavigateHomeAsync(CancellationToken cancellationToken)
        {
            IsNavigationVisible = false;
            await NavigationService.TryNavigateAsync(() => new HomeViewModel(_collectionStoreModel, this));
        }
    }
}
