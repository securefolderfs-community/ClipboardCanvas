using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Threading;

using ClipboardCanvas.Interfaces.Search;
using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.UserControls.Collections;
using System.Diagnostics;

namespace ClipboardCanvas.ViewModels
{
    public class CollectionPreviewItemViewModel : ObservableObject, ISearchItem, IDisposable
    {
        #region Private Members

        private CancellationTokenSource _cancellationTokenSource;

        #endregion

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

        public CollectionItemViewModel CollectionItemViewModel { get; set; }

        #endregion

        #region Constructor

        private CollectionPreviewItemViewModel()
        {
            this._cancellationTokenSource = new CancellationTokenSource();
        }

        #endregion

        #region Public Helpers

        public async Task RequestCanvasLoad()
        {
            // Wait for control to load
            await Task.Delay(Constants.UI.CONTROL_LOAD_DELAY);

            if (ReadOnlyCanvasPreviewModel == null)
            {
                Debug.WriteLine($"Item to load: {CollectionItemViewModel.AssociatedItem.Path} | CollecionModel: {CollectionModel}");
                Debugger.Break();
                return; // TODO: This is hit!
            }

            ReadOnlyCanvasPreviewModel.DiscardData();
            await ReadOnlyCanvasPreviewModel.TryLoadExistingData(CollectionItemViewModel, _cancellationTokenSource.Token);
        }

        public async Task RequestCanvasUnload()
        {
            _cancellationTokenSource.Cancel();

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

            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public static async Task<CollectionPreviewItemViewModel> GetCollectionPreviewItemModel(ICollectionModel collectionModel, CollectionItemViewModel collectionItemViewModel)
        {
            CollectionPreviewItemViewModel viewModel = new CollectionPreviewItemViewModel()
            {
                CollectionModel = collectionModel,
                CollectionItemViewModel = collectionItemViewModel
            };

            await viewModel.UpdateDisplayName();

            return viewModel;
        }

        public async Task UpdateDisplayName()
        {
            IStorageItem sourceItem = await CollectionItemViewModel.SourceItem;

            DisplayName = Path.GetFileName(sourceItem != null ? sourceItem.Path : CollectionItemViewModel.AssociatedItem?.Path);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
            ReadOnlyCanvasPreviewModel?.Dispose();
        }

        #endregion
    }
}
