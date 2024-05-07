using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Views
{
    public sealed partial class MainAppViewModel : ObservableObject, IAsyncInitialize, IViewable
    {
        [ObservableProperty] private string _Title;

        public INavigationService NavigationService { get; } = Ioc.Default.GetRequiredService<INavigationService>();

        public QuickOptionsViewModel QuickOptionsViewModel { get; } = Ioc.Default.GetRequiredService<QuickOptionsViewModel>();

        public RibbonViewModel RibbonViewModel { get; } = Ioc.Default.GetRequiredService<RibbonViewModel>();

        public NavigationViewModel NavigationViewModel { get; }

        public MainAppViewModel(ICollectionSourceModel collectionStoreModel)
        {
            NavigationViewModel = new(collectionStoreModel, NavigationService);
            Title = "Clipboard Canvas";

            NavigationService.NavigationChanged += NavigationService_NavigationChanged;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        private void NavigationService_NavigationChanged(object? sender, IViewDesignation? e)
        {
            Title = e?.Title ?? Title ?? "Clipboard Canvas";
        }
    }
}
