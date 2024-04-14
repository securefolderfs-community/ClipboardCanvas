using ClipboardCanvas.Sdk.AppModels;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Views;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.Shared.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels
{
    public sealed partial class MainViewModel : ObservableObject, IAsyncInitialize
    {
        [ObservableProperty] private MainAppViewModel _AppViewModel; // TODO: Limitation since the UI requires specific type for x:Bind on DependencyProperty

        public ICollectionSourceModel CollectionStoreModel { get; }

        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        public MainViewModel()
        {
            CollectionStoreModel = new CollectionSourceModel();
            _AppViewModel = new MainAppViewModel(CollectionStoreModel);
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.WhenAll(SettingsService.TryLoadAsync(cancellationToken), CollectionStoreModel.TryLoadAsync(cancellationToken));
        }
    }
}
