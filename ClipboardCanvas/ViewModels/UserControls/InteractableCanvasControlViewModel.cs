using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System;
using Windows.UI.Xaml;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.EventArguments.InfiniteCanvasEventArgs;
using ClipboardCanvas.Extensions;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class InteractableCanvasControlViewModel : ObservableObject, IInteractableCanvasControlModel, IDisposable
    {
        #region Private Members

        private readonly IInteractableCanvasControlView _view;

        private readonly DispatcherTimer _saveTimer;

        #endregion

        #region Public Properties

        public ObservableCollection<InteractableCanvasControlItemViewModel> Items { get; private set; }

        private bool _NoItemsTextLoad;
        public bool NoItemsTextLoad
        {
            get => _NoItemsTextLoad;
            set => SetProperty(ref _NoItemsTextLoad, value);
        }

        #endregion

        #region Events

        public event EventHandler<InfiniteCanvasSaveRequestedEventArgs> OnInfiniteCanvasSaveRequestedEvent;

        #endregion

        #region Constructor

        public InteractableCanvasControlViewModel(IInteractableCanvasControlView view)
        {
            this._view = view;

            this._saveTimer = new DispatcherTimer();
            this.Items = new ObservableCollection<InteractableCanvasControlItemViewModel>();

            this._saveTimer.Interval = TimeSpan.FromMilliseconds(Constants.UI.CanvasContent.INFINITE_CANVAS_SAVE_INTERVAL);
            this._saveTimer.Tick += SaveTimer_Tick;
        }

        #endregion

        #region Event Handlers

        private void SaveTimer_Tick(object sender, object e)
        {
            _saveTimer.Stop();
            OnInfiniteCanvasSaveRequestedEvent?.Invoke(this, new InfiniteCanvasSaveRequestedEventArgs());
        }

        #endregion

        #region Public Helpers

        public async Task<InteractableCanvasControlItemViewModel> AddItem(ICollectionModel collectionModel, BaseContentTypeModel contentType, CanvasItem canvasFile, CancellationToken cancellationToken)
        {
            var item = new InteractableCanvasControlItemViewModel(_view, collectionModel, contentType, canvasFile, cancellationToken);
            Items.Add(item);
            await item.InitializeItem();
            NoItemsTextLoad = false;

            return item;
        }

        public void RemoveItem(InteractableCanvasControlItemViewModel item)
        {
            Items.Remove(item);

            NoItemsTextLoad = Items.IsEmpty();
        }

        public InfiniteCanvasConfigurationModel GetConfigurationModel()
        {
            var canvasConfigurationModel = new InfiniteCanvasConfigurationModel();

            foreach (var item in Items)
            {
                canvasConfigurationModel.elements.Add(new InfiniteCanvasConfigurationItemModel(item.CanvasItem.AssociatedItem.Path, item.ItemPosition));
            }

            return canvasConfigurationModel;
        }

        public void SetConfigurationModel(InfiniteCanvasConfigurationModel canvasConfigurationModel)
        {
            foreach (var item1 in Items)
            {
                foreach (var item2 in canvasConfigurationModel.elements)
                {
                    if (item1.CanvasItem.AssociatedItem.Path == item2.associatedItemPath)
                    {
                        item1.ItemPosition = item2.locationVector;
                    }
                }
            }
        }
        public void CanvasLoaded()
        {
            NoItemsTextLoad = Items.IsEmpty();
        }

        public void ItemRearranged()
        {
            if (!_saveTimer.IsEnabled)
            {
                _saveTimer.Start();
            }
            else
            {
                _saveTimer.Stop();
                _saveTimer.Start();
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            this._saveTimer.Stop();
            this._saveTimer.Tick -= SaveTimer_Tick;
        }

        #endregion
    }
}
