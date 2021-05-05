using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Enums;
using System.Collections.Generic;
using Windows.Storage;
using System.IO;
using System.Threading;
using System.Linq;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class DynamicCanvasControlViewModel : ObservableObject, IPasteCanvasModel, IDisposable
    {
        #region Private Members

        private readonly IDynamicPasteCanvasControlView _view;

        #endregion

        #region Public Properties

        private BasePasteCanvasViewModel _CanvasViewModel;

        public BasePasteCanvasViewModel CanvasViewModel
        {
            get => _CanvasViewModel;
            set => SetProperty(ref _CanvasViewModel, value);
        }

        #endregion

        #region Static Members

        public static CancellationTokenSource CanvasPasteCancellationTokenSource = new CancellationTokenSource();

        #endregion

        #region IPasteCanvasEventsModel

        public event EventHandler<OpenOpenNewCanvasRequestedEventArgs> OnOpenNewCanvasRequestedEvent;
        public event EventHandler<ContentLoadedEventArgs> OnContentLoadedEvent;
        public event EventHandler<PasteRequestedEventArgs> OnPasteRequestedEvent;
        public event EventHandler<FileCreatedEventArgs> OnFileCreatedEvent;
        public event EventHandler<FileModifiedEventArgs> OnFileModifiedEvent;
        public event EventHandler<FileDeletedEventArgs> OnFileDeletedEvent;
        public event EventHandler<ErrorOccurredEventArgs> OnErrorOccurredEvent;

        #endregion

        #region Constructor

        public DynamicCanvasControlViewModel(IDynamicPasteCanvasControlView view)
        {
            this._view = view;
        }

        #endregion

        #region IPasteCanvasModel

        public async Task<SafeWrapperResult> TryLoadExistingData(ICollectionsContainerItemModel itemData, CancellationToken cancellationToken)
        {
            if (await InitializeViewModel(itemData))
            {
                return await CanvasViewModel?.TryLoadExistingData(itemData, cancellationToken);
            }

            SafeWrapperResult result = new SafeWrapperResult(OperationErrorCode.Unauthorized, new InvalidOperationException(), "Couldn't display content for this file");
            OnErrorOccurredEvent?.Invoke(this, new ErrorOccurredEventArgs(result, result.Details.message));

            return result;
        }

        public async Task<SafeWrapperResult> TryPasteData(DataPackageView dataPackage, CancellationToken cancellationToken)
        {
            if (await InitializeViewModel(dataPackage))
            {
                return await CanvasViewModel.TryPasteData(dataPackage, cancellationToken);
            }

            SafeWrapperResult result = new SafeWrapperResult(OperationErrorCode.Unauthorized, new InvalidOperationException(), "Couldn't display content for this file");
            OnErrorOccurredEvent?.Invoke(this, new ErrorOccurredEventArgs(result, result.Details.message));

            return result;
        }

        public async Task<SafeWrapperResult> TrySaveData()
        {
            return await CanvasViewModel?.TrySaveData();
        }

        public async Task<SafeWrapperResult> TryDeleteData()
        {
            return await CanvasViewModel?.TryDeleteData();
        }

        public void DiscardData()
        {
            CanvasViewModel?.DiscardData();
        }

        public void OpenNewCanvas()
        {
            CanvasViewModel?.OpenNewCanvas();
        }

        public Task<IEnumerable<SuggestedActionsControlItemViewModel>> GetSuggestedActions()
        {
            return CanvasViewModel?.GetSuggestedActions();
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Initialize View Model from data package
        /// </summary>
        /// <param name="dataPackage"></param>
        /// <returns></returns>
        private async Task<bool> InitializeViewModel(DataPackageView dataPackage)
        {
            // Decide content type and initialize view model
            if (dataPackage.Contains(StandardDataFormats.Bitmap))
            {
                CanvasViewModel = new ImageCanvasViewModel(_view);

                HookEvents();

                return true;
            }
            else if (dataPackage.Contains(StandardDataFormats.Text))
            {
                CanvasViewModel = new TextCanvasViewModel(_view);

                HookEvents();

                return true;
            }
            else if (dataPackage.Contains(StandardDataFormats.StorageItems))
            {
                IReadOnlyList<IStorageItem> items = await dataPackage.GetStorageItemsAsync();

                if (items.Count > 1)
                {
                    // TODO: More than one item, paste all files as reference
                }
                else if (items.Count == 1)
                {
                    // One item, decide view model for it
                    IStorageItem item = items.First();

                    BasePastedContentTypeDataModel contentType = await BasePastedContentTypeDataModel.GetContentType(item);

                    if (contentType == null)
                    {
                        return false;
                    }

                    return InitializeViewModel(contentType);
                }
                else
                {
                    // No items
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Initialize View Model from existing data
        /// </summary>
        private async Task<bool> InitializeViewModel(ICollectionsContainerItemModel containerItemModel)
        {
            DiscardData();

            // Decide content type
            BasePastedContentTypeDataModel contentType;

            if (containerItemModel.ContentType != null) // Reuse if not null
            {
                contentType = containerItemModel.ContentType;
            }
            else
            {
                // TODO: Add support for adding previews from extensions
                contentType = await BasePastedContentTypeDataModel.GetContentType(containerItemModel.File);
            }

            // Check if contentType is not null
            if (contentType == null)
            {
                return false;
            }

            // If containerItemModel.ContentType was null, assign it to reuse it later
            if (containerItemModel.ContentType == null)
            {
                containerItemModel.ContentType = contentType;
            }

            return InitializeViewModel(contentType);
        }

        private bool InitializeViewModel(BasePastedContentTypeDataModel contentType)
        {
            // Initialize View Model

            // Try for image
            if (InitializeViewModelForType<ImageContentType, ImageCanvasViewModel>(contentType, () => new ImageCanvasViewModel(_view)))
            {
                return true;
            }

            // Try for text
            if (InitializeViewModelForType<TextContentType, TextCanvasViewModel>(contentType, () => new TextCanvasViewModel(_view)))
            {
                return true;
            }

            // Try for media
            if (InitializeViewModelForType<MediaContentType, MediaCanvasViewModel>(contentType, () => new MediaCanvasViewModel(_view)))
            {
                return true;
            }

            return false;
        }

        private bool InitializeViewModelForType<TContentType, TViewModel>(BasePastedContentTypeDataModel contentType, Func<TViewModel> initializator)
            where TViewModel : BasePasteCanvasViewModel
            where TContentType : BasePastedContentTypeDataModel
        {
            if (contentType is TContentType)
            {
                if (CanvasViewModel is TViewModel) // Reuse View Model
                {
                    return true;
                }

                UnhookEvents();
                CanvasViewModel = initializator();
                HookEvents();

                return true;
            }

            return false;
        }

        #endregion

        #region Event Handlers

        private void CanvasViewModel_OnErrorOccurredEvent(object sender, ErrorOccurredEventArgs e)
        {
            OnErrorOccurredEvent?.Invoke(sender, e);
        }

        private void PasteCanvasControlModel_OnFileDeletedEvent(object sender, FileDeletedEventArgs e)
        {
            OnFileDeletedEvent?.Invoke(sender, e);
        }

        private void PasteCanvasControlModel_OnFileModifiedEvent(object sender, FileModifiedEventArgs e)
        {
            OnFileModifiedEvent?.Invoke(sender, e);
        }

        private void PasteCanvasControlModel_OnFileCreatedEvent(object sender, FileCreatedEventArgs e)
        {
            OnFileCreatedEvent?.Invoke(sender, e);
        }

        private void PasteCanvasControlModel_OnPasteRequestedEvent(object sender, PasteRequestedEventArgs e)
        {
            OnPasteRequestedEvent?.Invoke(sender, e);
        }

        private void PasteCanvasControlModel_OnContentLoadedEvent(object sender, ContentLoadedEventArgs e)
        {
            OnContentLoadedEvent?.Invoke(sender, e);
        }

        private void PasteCanvasControlModel_OnOpenNewCanvasRequestedEvent(object sender, OpenOpenNewCanvasRequestedEventArgs e)
        {
            OnOpenNewCanvasRequestedEvent?.Invoke(sender, e);
        }

        #endregion

        #region Event Hooks

        private void HookEvents()
        {
            UnhookEvents();
            if (CanvasViewModel != null)
            {
                CanvasViewModel.OnOpenNewCanvasRequestedEvent += PasteCanvasControlModel_OnOpenNewCanvasRequestedEvent;
                CanvasViewModel.OnContentLoadedEvent += PasteCanvasControlModel_OnContentLoadedEvent;
                CanvasViewModel.OnPasteRequestedEvent += PasteCanvasControlModel_OnPasteRequestedEvent;
                CanvasViewModel.OnFileCreatedEvent += PasteCanvasControlModel_OnFileCreatedEvent;
                CanvasViewModel.OnFileModifiedEvent += PasteCanvasControlModel_OnFileModifiedEvent;
                CanvasViewModel.OnFileDeletedEvent += PasteCanvasControlModel_OnFileDeletedEvent;
                CanvasViewModel.OnErrorOccurredEvent += CanvasViewModel_OnErrorOccurredEvent;
            }
        }

        private void UnhookEvents()
        {
            if (CanvasViewModel != null)
            {
                CanvasViewModel.OnOpenNewCanvasRequestedEvent -= PasteCanvasControlModel_OnOpenNewCanvasRequestedEvent;
                CanvasViewModel.OnContentLoadedEvent -= PasteCanvasControlModel_OnContentLoadedEvent;
                CanvasViewModel.OnPasteRequestedEvent -= PasteCanvasControlModel_OnPasteRequestedEvent;
                CanvasViewModel.OnFileCreatedEvent -= PasteCanvasControlModel_OnFileCreatedEvent;
                CanvasViewModel.OnFileModifiedEvent -= PasteCanvasControlModel_OnFileModifiedEvent;
                CanvasViewModel.OnFileDeletedEvent -= PasteCanvasControlModel_OnFileDeletedEvent;
                CanvasViewModel.OnErrorOccurredEvent -= CanvasViewModel_OnErrorOccurredEvent;
            }
        }

        #endregion

        #region IDisposable

        // TODO: Very important Dispose(). Please call it if events weren't unhook
        public void Dispose()
        {
            UnhookEvents();
            DiscardData();
        }

        #endregion
    }
}
