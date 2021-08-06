using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class InteractableCanvasControlViewModel : ObservableObject, IInteractableCanvasControlModel
    {
        #region Private Members

        private IInteractableCanvasControlView _view;

        #endregion

        #region Public Properties

        public ObservableCollection<InteractableCanvasControlItemViewModel> Items { get; private set; }

        #endregion

        #region Constructor

        public InteractableCanvasControlViewModel(IInteractableCanvasControlView view)
        {
            this._view = view;

            this.Items = new ObservableCollection<InteractableCanvasControlItemViewModel>();
        }

        #endregion

        #region IInteractableCanvasControlModel

        public async Task<InteractableCanvasControlItemViewModel> AddItem(ICollectionModel collectionModel, BaseContentTypeModel contentType, CanvasItem canvasFile, CancellationToken cancellationToken)
        {
            var item = new InteractableCanvasControlItemViewModel(_view, collectionModel, contentType, canvasFile, cancellationToken);
            Items.Add(item);
            await item.InitializeItem();

            return item;
        }

        #endregion
    }
}
