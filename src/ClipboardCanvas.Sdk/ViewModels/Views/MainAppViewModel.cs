using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using ClipboardCanvas.Sdk.Models;

namespace ClipboardCanvas.Sdk.ViewModels.Views
{
    public sealed partial class MainAppViewModel : ObservableObject, IAsyncInitialize
    {
        [ObservableProperty] private string _AppTitle;

        public INavigationService NavigationService { get; } = Ioc.Default.GetRequiredService<INavigationService>();

        public NavigationViewModel NavigationViewModel { get; }

        public MainAppViewModel(ICollectionStoreModel collectionStoreModel)
        {
            NavigationViewModel = new(collectionStoreModel, NavigationService);
            AppTitle = "Clipboard Canvas";

            NavigationService.NavigationChanged += NavigationService_NavigationChanged;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        private void NavigationService_NavigationChanged(object? sender, IViewDesignation? e)
        {
            AppTitle = e?.Title ?? AppTitle;
        }
    }
}
