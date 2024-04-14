using ClipboardCanvas.Sdk.ViewModels.Controls;
using ClipboardCanvas.Sdk.ViewModels.Widgets;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.Shared.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ClipboardCanvas.Sdk.Models;

namespace ClipboardCanvas.Sdk.ViewModels.Views
{
    public sealed partial class HomeViewModel : ObservableObject, IViewDesignation, IAsyncInitialize
    {
        private readonly ICollectionSourceModel _collectionStoreModel;
        private readonly NavigationViewModel _navigationViewModel;

        [ObservableProperty] private string? _Title;

        public ObservableCollection<BaseWidgetViewModel> Widgets { get; } = new();

        public HomeViewModel(ICollectionSourceModel collectionStoreModel, NavigationViewModel navigationViewModel)
        {
            _collectionStoreModel = collectionStoreModel;
            _navigationViewModel = navigationViewModel;
            Title = "Clipboard Canvas";
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            Widgets.Add(new CollectionsWidgetViewModel(_collectionStoreModel, _navigationViewModel).WithInitAsync(cancellationToken));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void OnAppearing()
        {
            _navigationViewModel.IsNavigationVisible = false;
        }

        /// <inheritdoc/>
        public void OnDisappearing()
        {
        }
    }
}
