using ClipboardCanvas.Sdk.Extensions;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClipboardCanvas.Sdk.ViewModels.Controls
{
    public sealed partial class NavigationViewModel : ObservableObject
    {
        private readonly ICollectionSourceModel _collectionStoreModel;

        [ObservableProperty] private ICommand? _NavigateBackCommand;
        [ObservableProperty] private ICommand? _NavigateForwardCommand;
        [ObservableProperty] private bool _IsNavigationVisible;
        [ObservableProperty] private bool _IsForwardEnabled;
        [ObservableProperty] private bool _IsBackEnabled;

        public INavigationService NavigationService { get; }

        public NavigationViewModel(ICollectionSourceModel collectionStoreModel, INavigationService navigationService)
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
            await NavigationService.TryNavigateAsync(() => new HomeViewModel(_collectionStoreModel, this));
        }
    }
}
