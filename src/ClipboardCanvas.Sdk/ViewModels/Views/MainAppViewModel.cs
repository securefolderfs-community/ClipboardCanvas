using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls;
using ClipboardCanvas.Sdk.ViewModels.Controls.Ribbon;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Views
{
    public sealed partial class MainAppViewModel : ObservableObject, IAsyncInitialize
    {
        [ObservableProperty] private string _AppTitle;

        public INavigationService NavigationService { get; } = Ioc.Default.GetRequiredService<INavigationService>();

        public RibbonViewModel RibbonViewModel { get; } = Ioc.Default.GetRequiredService<RibbonViewModel>();

        public NavigationViewModel NavigationViewModel { get; }

        public MainAppViewModel(ICollectionSourceModel collectionStoreModel)
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
