using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.ContextMenu;
using ClipboardCanvas.Enums;
using ClipboardCanvas.ViewModels.UserControls.SimpleCanvasDisplay;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class InteractableCanvasControlItemViewModel : ObservableObject, IInteractableCanvasControlItemModel, IDisposable
    {
        #region Private Members

        private IInteractableCanvasControlView _view;

        private BaseContentTypeModel _contentType;

        private CanvasItem _canvasItem;

        private CancellationToken _cancellationToken;

        #endregion

        #region Properties

        public List<BaseMenuFlyoutItemViewModel> ContextMenuItems { get; private set; }

        public IReadOnlyCanvasPreviewModel ReadOnlyCanvasPreviewModel { get; set; }

        public ICollectionModel CollectionModel { get; set; }

        private bool _IsPastedAsReference;
        public bool IsPastedAsReference
        {
            get => _IsPastedAsReference;
            set => SetProperty(ref _IsPastedAsReference, value);
        }

        private string _DisplayName;
        public string DisplayName
        {
            get => _DisplayName;
            set => SetProperty(ref _DisplayName, value);
        }

        private Vector2 ItemPosition
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

        #region Constructor

        public InteractableCanvasControlItemViewModel(IInteractableCanvasControlView view, ICollectionModel collectionModel, BaseContentTypeModel contentType, CanvasItem canvasItem, CancellationToken cancellationToken)
        {
            this._view = view;
            this.CollectionModel = collectionModel;
            this._contentType = contentType;
            this._canvasItem = canvasItem;
            this._cancellationToken = cancellationToken;
        }

        #endregion

        public async Task InitializeItem()
        {
            DisplayName = (await _canvasItem.SourceItem).Name;
        }

        public async Task<SafeWrapperResult> LoadContent()
        {
            SafeWrapperResult result = await ReadOnlyCanvasPreviewModel.TryLoadExistingData(_canvasItem, _contentType, _cancellationToken);
            IsPastedAsReference = result && _canvasItem.IsFileAsReference;

            return result;
        }

        public async Task<IReadOnlyList<IStorageItem>> GetDragData()
        {
            return new List<IStorageItem>() { await _canvasItem.SourceItem };
        }

        #region IDisposable

        public void Dispose()
        {
            _view = null;
        }

        #endregion
    }
}
