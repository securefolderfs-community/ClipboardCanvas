using ClipboardCanvas.Sdk.Extensions;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Views
{
    public sealed partial class MainAppViewModel : ObservableObject, IAsyncInitialize, IViewable
    {
        private readonly ICollectionSourceModel _collectionSourceModel;

        [ObservableProperty] private string _Title;

        public ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        public INavigationService NavigationService { get; } = Ioc.Default.GetRequiredService<INavigationService>();

        public QuickOptionsViewModel QuickOptionsViewModel { get; } = Ioc.Default.GetRequiredService<QuickOptionsViewModel>();

        public RibbonViewModel RibbonViewModel { get; } = Ioc.Default.GetRequiredService<RibbonViewModel>();

        public NavigationViewModel NavigationViewModel { get; }

        public MainAppViewModel(ICollectionSourceModel collectionSourceModel)
        {
            _collectionSourceModel = collectionSourceModel;
            NavigationViewModel = new(collectionSourceModel, NavigationService);
            Title = "Clipboard Canvas";

            NavigationService.NavigationChanged += NavigationService_NavigationChanged;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (SettingsService.UserSettings.UninterruptedResume)
            {
                var parentId = Path.GetDirectoryName(SettingsService.AppSettings.LastLocationId);
                foreach (var collection in _collectionSourceModel)
                {
                    if (collection.Source.Id == parentId)
                    {
                        // TODO: Figure out a way to navigate to canvas without (or with?) collections initialized
                        await NavigationService.NavigateAsync((CanvasViewModel?)null!);
                        break;
                    }
                }
            }
            else
                await NavigationService.TryNavigateAsync(() => new HomeViewModel(_collectionSourceModel, NavigationViewModel));
        }

        private void NavigationService_NavigationChanged(object? sender, IViewDesignation? e)
        {
            Title = e?.Title ?? Title ?? "Clipboard Canvas";
        }
    }
}
