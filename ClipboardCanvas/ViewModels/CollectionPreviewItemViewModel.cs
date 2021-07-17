using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml;

using ClipboardCanvas.Interfaces.Search;
using ClipboardCanvas.Models;
using ClipboardCanvas.Models.Implementation;
using ClipboardCanvas.ViewModels.Pages;
using System.Diagnostics;

namespace ClipboardCanvas.ViewModels
{
    public class CollectionPreviewItemViewModel : ObservableObject, ISearchItem, IDisposable
    {
        #region Public Properties

        private string _DisplayName;
        public string DisplayName
        {
            get => _DisplayName;
            private set => SetProperty(ref _DisplayName, value);
        }

        private bool _IsSelected;
        public bool IsSelected
        {
            get => _IsSelected;
            set => SetProperty(ref _IsSelected, value);
        }

        private bool _IsHighlighted;
        public bool IsHighlighted
        {
            get => _IsHighlighted;
            set => SetProperty(ref _IsHighlighted, value);
        }

        private bool _IsCanvasPreviewVisible;
        public bool IsCanvasPreviewVisible
        {
            get => _IsCanvasPreviewVisible;
            set => SetProperty(ref _IsCanvasPreviewVisible, value);
        }

        public IReadOnlyCanvasPreviewModel SimpleCanvasPreviewModel { get; set; }

        public ICollectionModel CollectionModel { get; private set; }

        public ICollectionItemModel CollectionItemModel { get; private set; }

        #endregion

        #region Commands

        public ICommand SimpleCanvasLoadedCommand { get; set; }

        #endregion

        #region Constructor

        public CollectionPreviewItemViewModel()
        {
            // Create commands
            SimpleCanvasLoadedCommand = new RelayCommand<RoutedEventArgs>(SimpleCanvasLoaded);
        }

        #endregion

        #region Command Implementation


        private async void SimpleCanvasLoaded(RoutedEventArgs e)
        {
            if (_loadRequested)
            {
                await SimpleCanvasPreviewModel.TryLoadExistingData(CollectionItemModel, CollectionPreviewPageViewModel.LoadCancellationToken.Token);
                IsCanvasPreviewVisible = true;
                Debug.WriteLine("LOADED VIA LOAD");
            }
            _loadRequested = false;
        }

        #endregion

        #region Public Helpers

        private bool _loadRequested;

        public async Task RequestCanvasLoad()
        {
            if (SimpleCanvasPreviewModel == null)
            {
                _loadRequested = true;
                return;
            }

            await SimpleCanvasPreviewModel.TryLoadExistingData(CollectionItemModel, CollectionPreviewPageViewModel.LoadCancellationToken.Token);
            Debug.WriteLine("LOADED VIA REQUEST");
        }

        public static async Task<CollectionPreviewItemViewModel> GetCollectionPreviewItemModel(ICollectionModel collectionModel, ICollectionItemModel collectionItemModel)
        {
            CollectionPreviewItemViewModel viewModel = new CollectionPreviewItemViewModel()
            {
                CollectionModel = collectionModel,
                CollectionItemModel = collectionItemModel
            };

            IStorageItem sourceItem = await collectionItemModel.SourceItem;

            viewModel.DisplayName = Path.GetFileName(sourceItem.Path);

            return viewModel;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            SimpleCanvasPreviewModel?.Dispose();
        }

        #endregion
    }
}
