using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;
using Windows.Storage;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.Toolkit.Uwp;
using System.Collections.ObjectModel;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.DataModels.ContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.EventArguments.InfiniteCanvasEventArgs;
using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.ViewModels.UserControls.InAppNotifications;
using ClipboardCanvas.Services;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.ViewModels.ContextMenu;
using ClipboardCanvas.EventArguments.CanvasControl;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class InteractableCanvasControlItemViewModel : ObservableObject, IInteractableCanvasControlItemModel, IDisposable
    {
        #region Private Members

        private IInteractableCanvasControlView _view;

        private readonly BaseContentTypeModel _contentType;

        private CancellationToken _cancellationToken;

        private readonly ICanvasItemReceiverModel _inifinteCanvasFileReceiver;

        #endregion

        #region Properties

        private IDialogService DialogService { get; } = Ioc.Default.GetService<IDialogService>();

        public IReadOnlyCanvasPreviewModel ReadOnlyCanvasPreviewModel { get; set; }

        public ICollectionModel CollectionModel { get; set; }

        public CanvasItem CanvasItem { get; private set; }

        public ObservableCollection<BaseMenuFlyoutItemViewModel> CanvasContextMenuItems => ReadOnlyCanvasPreviewModel?.ContextMenuItems;

        private bool _IsPastedAsReference;
        public bool IsPastedAsReference
        {
            get => _IsPastedAsReference;
            set => SetProperty(ref _IsPastedAsReference, value);
        }

        private bool _OverrideReferenceEnabled;
        public bool OverrideReferenceEnabled
        {
            get => _OverrideReferenceEnabled;
            set => SetProperty(ref _OverrideReferenceEnabled, value);
        }

        private string _DisplayName;
        public string DisplayName
        {
            get => _DisplayName;
            set => SetProperty(ref _DisplayName, value);
        }

        public int CanvasTopIndex
        {
            get => _view.GetCanvasTopIndex(this);
            set => _view.SetCanvasTopIndex(this, value);
        }

        /// <summary>
        /// The item position
        /// </summary>
        public Vector2 ItemPosition
        {
            get => _view.GetItemPosition(this);
            set => _view.SetItemPosition(this, value);
        }

        /// <summary>
        /// The horizontal position
        /// </summary>
        public float XPos
        {
            get => ItemPosition.X;
            set => ItemPosition = new Vector2(value, ItemPosition.Y);
        }

        /// <summary>
        /// The vertical position
        /// </summary>
        public float YPos
        {
            get => ItemPosition.Y;
            set => ItemPosition = new Vector2(ItemPosition.X, value);
        }

        #endregion

        public event EventHandler<InfiniteCanvasItemRemovalRequestedEventArgs> OnInfiniteCanvasItemRemovalRequestedEvent;

        public ICommand OverrideReferenceCommand { get; private set; }

        #region Constructor

        internal InteractableCanvasControlItemViewModel(IInteractableCanvasControlView view, ICollectionModel collectionModel, BaseContentTypeModel contentType, CanvasItem canvasItem, ICanvasItemReceiverModel inifinteCanvasFileReceiver, CancellationToken cancellationToken)
        {
            this._view = view;
            this.CollectionModel = collectionModel;
            this._contentType = contentType;
            this.CanvasItem = canvasItem;
            this._inifinteCanvasFileReceiver = inifinteCanvasFileReceiver;
            this._cancellationToken = cancellationToken;

            // Create commands
            OverrideReferenceCommand = new AsyncRelayCommand(OverrideReference);
        }

        #endregion

        #region Event Handlers

        private void ReadOnlyCanvasPreviewModel_OnFileDeletedEvent(object sender, FileDeletedEventArgs e)
        {
            OnInfiniteCanvasItemRemovalRequestedEvent?.Invoke(this, new InfiniteCanvasItemRemovalRequestedEventArgs(this));
        }

        #endregion

        #region Helpers

        private async Task OverrideReference()
        {
            if (!IsPastedAsReference)
            {
                return;
            }

            OverrideReferenceEnabled = false;
            SafeWrapper<CanvasItem> newCanvasItemResult = await this.ReadOnlyCanvasPreviewModel.PasteOverrideReference();
            OverrideReferenceEnabled = true;

            if (newCanvasItemResult)
            {
                this.CanvasItem = newCanvasItemResult;
                IsPastedAsReference = false;
            }
            else if (newCanvasItemResult != OperationErrorCode.Canceled)
            {
                IInAppNotification notification = DialogService.GetNotification();
                notification.ViewModel.NotificationText = string.Format("ErrorWhilstOverridingReference".GetLocalized(), newCanvasItemResult.ErrorCode);
                notification.ViewModel.ShownButtons = InAppNotificationButtonType.OkButton;

                notification.Show(Constants.UI.Notifications.NOTIFICATION_DEFAULT_SHOW_TIME);
            }
        }

        public async Task OpenFile()
        {
            await StorageHelpers.OpenFile(await CanvasItem.SourceItem);
        }

        public async Task InitializeDisplayName()
        {
            DisplayName = (await CanvasItem.SourceItem)?.Name ?? "InvalidFile".GetLocalized();
        }

        public async Task InitializeItem()
        {
             await InitializeDisplayName();
             OverrideReferenceEnabled = (await CanvasItem.SourceItem) is not StorageFolder;
        }

        public async Task<SafeWrapperResult> LoadContent(bool withLoadDelay = false)
        {
            if (withLoadDelay)
            {
                // Wait for control to load
                await Task.Delay(Constants.UI.CONTROL_LOAD_DELAY);
            }

            SafeWrapperResult result;
            if (ReadOnlyCanvasPreviewModel != null)
            {
                result = await ReadOnlyCanvasPreviewModel.TryLoadExistingData(CanvasItem, _contentType, _cancellationToken);

                if (result)
                {
                    OnPropertyChanged(nameof(CanvasContextMenuItems));

                    ReadOnlyCanvasPreviewModel.OnFileDeletedEvent += ReadOnlyCanvasPreviewModel_OnFileDeletedEvent;
                    IsPastedAsReference = CanvasItem.IsFileAsReference;
                    ReadOnlyCanvasPreviewModel.CanvasItemReceiver = _inifinteCanvasFileReceiver;
                }
            }
            else
            {
                return SafeWrapperResult.CANCEL;
            }

            return result;
        }

        public async Task SetDragData(DataPackage data)
        {
            bool dataSet = false;

            if (ReadOnlyCanvasPreviewModel != null)
            {
                dataSet = await ReadOnlyCanvasPreviewModel.SetDataToDataPackage(data);
            }

            if (!dataSet)
            {
                data.SetStorageItems((await CanvasItem.SourceItem).ToListSingle());
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (ReadOnlyCanvasPreviewModel != null)
            {
                ReadOnlyCanvasPreviewModel.OnFileDeletedEvent -= ReadOnlyCanvasPreviewModel_OnFileDeletedEvent;
            }

            _view = null;
        }

        #endregion
    }
}
