using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Views
{
    public sealed partial class MainAppViewModel : ObservableObject, IAsyncInitialize
    {
        public INavigationService NavigationService { get; } = Ioc.Default.GetRequiredService<INavigationService>();

        public NavigationViewModel NavigationViewModel { get; }

        public MainAppViewModel()
        {
            NavigationViewModel = new(NavigationService);
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
