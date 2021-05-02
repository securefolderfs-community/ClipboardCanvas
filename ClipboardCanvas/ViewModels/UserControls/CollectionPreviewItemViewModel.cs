using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class CollectionPreviewItemViewModel : ObservableObject
    {
        #region Public Properties

        private IPasteCanvasModel _AssociatedCanvas;
        public IPasteCanvasModel AssociatedCanvas
        {
            get => _AssociatedCanvas;
            set => SetProperty(ref _AssociatedCanvas, value);
        }

        private ICollectionsContainerModel _ParentCollection;
        public ICollectionsContainerModel ParentCollection 
        {
            get => _ParentCollection;
            set => SetProperty(ref _ParentCollection, value);
        }

        private string _ItemName;
        public string ItemName
        {
            get => _ItemName;
            set => SetProperty(ref _ItemName, value);
        }

        #endregion

        #region Events

        public event EventHandler<CanvasRenameRequestedEventArgs> OnCanvasRenameRequestedEvent;

        #endregion

        #region Constructor

        public CollectionPreviewItemViewModel(ICollectionsContainerModel parentCollection, string itemName)
        {
            this.ParentCollection = parentCollection;
            this.ItemName = itemName;

            Initialize();
        }

        #endregion

        #region Private Helpers

        private async void Initialize()
        {
            await Task.Delay(1000);
            Debug.WriteLine(AssociatedCanvas);
        }

        #endregion
    }
}
