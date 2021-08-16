using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Toolkit.Mvvm.ComponentModel;

using ClipboardCanvas.Interfaces.Search;
using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.Pages;
using ClipboardCanvas.ViewModels.UserControls.Collections;

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

        public IReadOnlyCanvasPreviewModel ReadOnlyCanvasPreviewModel { get; set; }

        public ICollectionModel CollectionModel { get; private set; }

        public CollectionItemViewModel CollectionItemViewModel { get; private set; }

        #endregion

        #region Public Helpers

        public async Task RequestCanvasLoad()
        {
            // Wait for control to load
            await Task.Delay(Constants.UI.CONTROL_LOAD_DELAY);

            if (ReadOnlyCanvasPreviewModel == null)
            {
                return;
            }

            ReadOnlyCanvasPreviewModel.DiscardData();
            await ReadOnlyCanvasPreviewModel.TryLoadExistingData(CollectionItemViewModel, CollectionPreviewPageViewModel.LoadCancellationToken.Token);
        }

        public async Task RequestCanvasUnload()
        {
            if (ReadOnlyCanvasPreviewModel == null)
            {
                // Wait for control to load if we unload too quickly
                await Task.Delay(Constants.UI.CONTROL_LOAD_DELAY);

                if (ReadOnlyCanvasPreviewModel == null)
                {
                    return;
                }
            }

            ReadOnlyCanvasPreviewModel.DiscardData();
        }

        public static async Task<CollectionPreviewItemViewModel> GetCollectionPreviewItemModel(ICollectionModel collectionModel, CollectionItemViewModel collectionItemViewModel)
        {
            CollectionPreviewItemViewModel viewModel = new CollectionPreviewItemViewModel()
            {
                CollectionModel = collectionModel,
                CollectionItemViewModel = collectionItemViewModel
            };

            IStorageItem sourceItem = await collectionItemViewModel.SourceItem;

            viewModel.DisplayName = Path.GetFileName(sourceItem != null ? sourceItem.Path : collectionItemViewModel.AssociatedItem?.Path);

            return viewModel;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            ReadOnlyCanvasPreviewModel?.Dispose();
        }

        #endregion
    }
}
