using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using System.Numerics;
using Windows.ApplicationModel.DataTransfer;
using System.Linq;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.EventArguments.InfiniteCanvasEventArgs;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.CanvasFileReceivers;

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

        public InteractableCanvasDataPackageComparisionDataModel DataPackageComparisionDataModel { get; set; }

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

        private async void SaveTimer_Tick(object sender, object e)
        {
            _saveTimer.Stop();

            await SaveCanvas();
        }

        private void Item_OnInfiniteCanvasItemRemovalRequestedEvent(object sender, InfiniteCanvasItemRemovalRequestedEventArgs e)
        {
            RemoveItem(e.itemToRemove);
        }

        #endregion

        #region Private Helpers

        private async Task SaveCanvas()
        {
            (IBuffer buffer, uint pixelWidth, uint pixelHeight) = await _view.GetCanvasImageBuffer();

            if (buffer != null)
            {
                OnInfiniteCanvasSaveRequestedEvent?.Invoke(this, new InfiniteCanvasSaveRequestedEventArgs(buffer, pixelWidth, pixelHeight));
            }
        }

        #endregion

        #region Public Helpers

        public async Task<InteractableCanvasControlItemViewModel> AddItem(ICollectionModel collectionModel, BaseContentTypeModel contentType, CanvasItem canvasItem, ICanvasItemReceiverModel inifinteCanvasFileReceiver, CancellationToken cancellationToken)
        {
            bool isDuplicate = Items.Any((item) => item.CanvasItem.AssociatedItem == canvasItem.AssociatedItem);
            if (isDuplicate)
            {
                return null;
            }

            var item = new InteractableCanvasControlItemViewModel(_view, collectionModel, contentType, canvasItem, inifinteCanvasFileReceiver, cancellationToken);
            item.OnInfiniteCanvasItemRemovalRequestedEvent += Item_OnInfiniteCanvasItemRemovalRequestedEvent;
            Items.Add(item);
            await item.InitializeItem();
            NoItemsTextLoad = false;

            _view?.SetCanvasTopIndex(item, Items.Count);

            return item;
        }

        public void RemoveItem(InteractableCanvasControlItemViewModel item)
        {
            if (item != null)
            {
                item.OnInfiniteCanvasItemRemovalRequestedEvent -= Item_OnInfiniteCanvasItemRemovalRequestedEvent;
                Items.Remove(item);

                NoItemsTextLoad = Items.IsEmpty();
            }
        }

        public bool ContainsItem(InteractableCanvasControlItemViewModel item)
        {
            if (item == null)
            {
                return false;
            }

            return Items.Any((item2) => item2 == item);
        }

        public InteractableCanvasControlItemViewModel FindItem(string path)
        {
            return Items.FirstOrDefault((item) => item.CanvasItem.AssociatedItem.Path == path);
        }

        public InfiniteCanvasConfigurationModel ConstructConfigurationModel()
        {
            var canvasConfigurationModel = new InfiniteCanvasConfigurationModel();

            foreach (var item in Items)
            {
                canvasConfigurationModel.elements.Add(new InfiniteCanvasConfigurationItemModel(item.CanvasItem.AssociatedItem.Path, item.ItemPosition, item.CanvasTopIndex));
            }

            return canvasConfigurationModel;
        }

        public void SetConfigurationModel(InfiniteCanvasConfigurationModel canvasConfigurationModel)
        {
            if (canvasConfigurationModel == null)
            {
                return;
            }

            foreach (var item1 in Items)
            {
                foreach (var item2 in canvasConfigurationModel.elements)
                {
                    if (item1.CanvasItem.AssociatedItem.Path == item2.associatedItemPath)
                    {
                        item1.ItemPosition = item2.locationVector;
                        item1.CanvasTopIndex = item2.canvasTopIndex;
                    }
                }
            }

            NoItemsTextLoad = Items.IsEmpty();
        }

        public void UpdateItemPositionFromDataPackage(DataPackageView dataPackage, InteractableCanvasControlItemViewModel interactableCanvasControlItem)
        {
            // If saved dataPackage == dataPackage from InfiniteCanvasViewModel
            if (DataPackageComparisionDataModel?.dataPackage == dataPackage)
            {
                // Set the position
                interactableCanvasControlItem.ItemPosition = new Vector2((float)DataPackageComparisionDataModel.dropPoint.X, (float)DataPackageComparisionDataModel.dropPoint.Y);

                // We don't need it anymore, null it so it doesn't cause trouble
                DataPackageComparisionDataModel = null;
            }
        }

        public async Task ResetAllItemPositions()
        {
            foreach (var item in Items)
            {
                item.ItemPosition = new Vector2(0.0f, 0.0f);
            }

            await SaveCanvas();
        }

        public async Task RegenerateCanvasPreview()
        {
            await SaveCanvas();
        }

        public async Task CanvasLoaded()
        {
            // Await so text doesn't flash
            await Task.Delay(Constants.UI.CONTROL_LOAD_DELAY);

            // Items might change after the delay
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
            for (int i = 0; i < Items.Count; i++)
            {
                var currentItem = Items[i];

                currentItem.OnInfiniteCanvasItemRemovalRequestedEvent -= Item_OnInfiniteCanvasItemRemovalRequestedEvent;
                currentItem.Dispose();
            }

            Items.Clear();

            this._saveTimer.Stop();
            this._saveTimer.Tick -= SaveTimer_Tick;
        }

        #endregion
    }
}
