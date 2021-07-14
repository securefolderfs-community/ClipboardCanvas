using ClipboardCanvas.Interfaces.Search;
using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.UserControls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.ViewModels
{
    public class CollectionPreviewItemViewModel : ObservableObject, ISearchItem
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

        public ICollectionItemModel CollectionItemModel { get; private set; }

        #endregion

        #region Constructor

        private CollectionPreviewItemViewModel()
        {
        }

        #endregion

        #region Public Helpers

        public static async Task<CollectionPreviewItemViewModel> GetCollectionPreviewItemModel(ICollectionItemModel collectionItemModel)
        {
            CollectionPreviewItemViewModel viewModel = new CollectionPreviewItemViewModel()
            {
                CollectionItemModel = collectionItemModel
            };
            IStorageItem sourceItem = await collectionItemModel.SourceItem;
            string itemPath;

            if (sourceItem == null)
            {
                itemPath = viewModel.CollectionItemModel.Item.Path;
            }
            else
            {
                itemPath = sourceItem.Path;
            }

            viewModel.DisplayName = Path.GetFileName(itemPath);

            return viewModel;
        }

        #endregion
    }
}
