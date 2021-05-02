using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.ViewModels.Pages
{
    public class CollectionPreviewPageViewModel : ObservableObject
    {
        #region Singleton

        private ICollectionsContainerModel AssociatedCollection => _view?.AssociatedCollection;

        #endregion

        #region Private Members

        private readonly ICollectionPreviewPageView _view;

        #endregion

        #region Public Properties

        public ObservableCollection<CollectionPreviewItemViewModel> Items { get; private set; }

        #endregion

        #region Constructor

        public CollectionPreviewPageViewModel(ICollectionPreviewPageView view)
        {
            this._view = view;

            Items = new ObservableCollection<CollectionPreviewItemViewModel>();
        }

        #endregion

        #region Public Helpers

        public void Initialize()
        {
            InitItems();
        }

        #endregion

        #region Private Helpers

        private void InitItems()
        {
            foreach (var item in AssociatedCollection.Items)
            {
                CollectionPreviewItemViewModel collectionPreviewItem = new CollectionPreviewItemViewModel(AssociatedCollection, item.File.Name);
                Items.Add(collectionPreviewItem);
            }
        }

        #endregion
    }
}
