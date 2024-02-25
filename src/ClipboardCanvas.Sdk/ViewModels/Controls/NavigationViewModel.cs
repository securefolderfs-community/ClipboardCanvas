using ClipboardCanvas.Sdk.Extensions;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading;
using System.Threading.Tasks;
using ClipboardCanvas.Sdk.Models;

namespace ClipboardCanvas.Sdk.ViewModels.Controls
{
    public sealed partial class NavigationViewModel : ObservableObject
    {
        private readonly ICollectionStoreModel _collectionStoreModel;

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
            await NavigationService.TryNavigateAsync(() => new HomeViewModel(_collectionStoreModel, this));
        }
    }
}
