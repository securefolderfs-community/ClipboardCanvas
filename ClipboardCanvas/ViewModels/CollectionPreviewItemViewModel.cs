using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Printing3D;
using Windows.Storage;
using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.Contexts.Operations;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.DataModels.ContentDataModels;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.GlobalizationExtensions;
using ClipboardCanvas.Interfaces.Search;
using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.UserControls.Collections;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.ReferenceItems;
using ClipboardCanvas.Services;
using ClipboardCanvas.ViewModels.ContextMenu;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace ClipboardCanvas.ViewModels
{
    public class CollectionPreviewItemViewModel : ObservableObject, ISearchItem, IDisposable
    {
        #region Private Members

        private CancellationTokenSource _cancellationTokenSource;

        private IUserSettingsService UserSettingsService { get; } = Ioc.Default.GetRequiredService<IUserSettingsService>();

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

        public ObservableCollection<BaseMenuFlyoutItemViewModel> ContextMenuItems { get; }


        public TwoWayPropertyUpdater<IReadOnlyCanvasPreviewModel> TwoWayReadOnlyCanvasPreview { get; set; }

        public IReadOnlyCanvasPreviewModel ReadOnlyCanvasPreviewModel { get; private set; }

        public ICollectionModel CollectionModel { get; private set; }

        public CollectionItemViewModel CollectionItemViewModel { get; set; }

        #endregion

        #region Constructor

        private CollectionPreviewItemViewModel()
        {
            this._cancellationTokenSource = new CancellationTokenSource();
            this.ContextMenuItems = new();

            this.TwoWayReadOnlyCanvasPreview = new TwoWayPropertyUpdater<IReadOnlyCanvasPreviewModel>();
            this.TwoWayReadOnlyCanvasPreview.OnPropertyValueUpdatedEvent += TwoWayReadOnlyCanvasPreview_OnPropertyValueUpdatedEvent;
        }

        #endregion

        #region Event Handlers

        private void TwoWayReadOnlyCanvasPreview_OnPropertyValueUpdatedEvent(object sender, IReadOnlyCanvasPreviewModel e)
        {
            ReadOnlyCanvasPreviewModel = e;
        }

        #endregion

        #region Public Helpers

        public async Task RequestCanvasLoad()
        {
            // Wait for control to load
            if (ReadOnlyCanvasPreviewModel == null)
            {
                await Task.Delay(Constants.UI.CONTROL_LOAD_DELAY);
                if (ReadOnlyCanvasPreviewModel == null)
                {
                    return;
                }
            }

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

            await viewModel.LoadContextMenuItems();
            await viewModel.UpdateDisplayName();

            return viewModel;
        }

        public async Task UpdateDisplayName()
        {
            DisplayName = Path.GetFileName(await CanvasHelpers.SafeGetCanvasItemPath(CollectionItemViewModel));
        }

        #endregion

        #region Private Helpers

        private async Task LoadContextMenuItems()
        {
            if (!ContextMenuItems.IsEmpty())
                return;

            var sourceItem = await CollectionItemViewModel.SourceItem;
            if (sourceItem is IStorageFolder && sourceItem.Name.EndsWith(Constants.FileSystem.INFINITE_CANVAS_EXTENSION))
            {
                ContextMenuItems.Add(new MenuFlyoutItemViewModel()
                {
                    Command = new AsyncRelayCommand(PasteToInfiniteCanvas),
                    IconGlyph = "\uE77F",
                    Text = "PasteFromClipboard".GetLocalized2()
                });
            }
            else
            {
                ContextMenuItems.Add(new MenuFlyoutItemViewModel()
                {
                    Command = new AsyncRelayCommand(CopyFile),
                    IconGlyph = "\uE8C8",
                    Text = "CopyFile".GetLocalized2()
                });
            }
        }

        private async Task CopyFile()
        {
            DataPackage data = new DataPackage();
            bool result = await ReadOnlyCanvasPreviewModel.SetDataToDataPackage(data);
            ClipboardHelpers.CopyDataPackage(data);
        }

        private async Task PasteToInfiniteCanvas()
        {
            var clipboardData = ClipboardHelpers.GetClipboardData();
            if (!clipboardData)
                return;

            var pastedItemContentType = await BaseContentTypeModel.GetContentTypeFromDataPackage(clipboardData);
            if (pastedItemContentType is InvalidContentTypeDataModel)
                return;

            var canvasItem = new InfiniteCanvasItem(CollectionItemViewModel.AssociatedItem);
            var infiniteCanvasFileReceiver = new InfiniteCanvasFileReceiver(canvasItem);
            var canvasPasteModel = CanvasHelpers.GetPasteModelFromContentType(pastedItemContentType, infiniteCanvasFileReceiver, new StatusCenterOperationReceiver());
            await canvasPasteModel.PasteData(clipboardData, UserSettingsService.AlwaysPasteFilesAsReference, default);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            ReadOnlyCanvasPreviewModel?.Dispose();
            
            if (TwoWayReadOnlyCanvasPreview != null)
            {
                this.TwoWayReadOnlyCanvasPreview.OnPropertyValueUpdatedEvent -= TwoWayReadOnlyCanvasPreview_OnPropertyValueUpdatedEvent;
            }
        }

        #endregion
    }
}
