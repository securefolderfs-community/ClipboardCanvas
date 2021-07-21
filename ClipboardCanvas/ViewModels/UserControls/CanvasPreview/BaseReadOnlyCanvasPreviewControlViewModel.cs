using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Enums;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.ContextMenu;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.DataModels;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasPreview
{
    public abstract class BaseReadOnlyCanvasPreviewControlViewModel<TBaseViewModel> : ObservableObject, IReadOnlyCanvasPreviewModel, IDisposable
        where TBaseViewModel : BaseReadOnlyCanvasViewModel
    {
        #region Protected Members

        protected ICollectionModel collectionModel => view?.CollectionModel;

        protected CollectionItemViewModel collectionItemViewModel;

        protected readonly IBaseCanvasPreviewControlView view;

        protected IStorageItem associatedItem => collectionItemViewModel.AssociatedItem;

        protected readonly SafeWrapperResult CanvasNullResult = new SafeWrapperResult(OperationErrorCode.InvalidArgument, new NullReferenceException(), "Invalid Canvas.");

        #endregion

        #region Public Properties

        private TBaseViewModel _CanvasViewModel;
        public TBaseViewModel CanvasViewModel
        {
            get => _CanvasViewModel;
            set => SetProperty(ref _CanvasViewModel, value);
        }

        public bool IsContentLoaded => CanvasViewModel?.IsContentLoaded ?? false;

        public List<BaseMenuFlyoutItemViewModel> ContextMenuItems => CanvasViewModel?.ContextMenuItems;

        #endregion

        #region Static Members

        public static CancellationTokenSource CanvasPasteCancellationTokenSource = new CancellationTokenSource();

        #endregion

        #region Events

        public event EventHandler<ContentStartedLoadingEventArgs> OnContentStartedLoadingEvent;

        public event EventHandler<ContentLoadedEventArgs> OnContentLoadedEvent;

        public event EventHandler<FileDeletedEventArgs> OnFileDeletedEvent;

        public event EventHandler<ErrorOccurredEventArgs> OnErrorOccurredEvent;

        public event EventHandler<TipTextUpdateRequestedEventArgs> OnTipTextUpdateRequestedEvent;

        public event EventHandler<ProgressReportedEventArgs> OnProgressReportedEvent;

        #endregion

        #region Constructor

        public BaseReadOnlyCanvasPreviewControlViewModel(IBaseCanvasPreviewControlView view)
        {
            this.view = view;
        }

        #endregion

        #region IReadOnlyCanvasPreviewModel

        public virtual async Task<SafeWrapperResult> TryLoadExistingData(CollectionItemViewModel itemData, CancellationToken cancellationToken)
        {
            this.collectionItemViewModel = itemData;

            SafeWrapperResult result = await InitializeViewModelFromCollectionItem(itemData, itemData.ContentType);

            if (result && CanvasViewModel != null)
            {
                return await CanvasViewModel.TryLoadExistingData(itemData, cancellationToken);
            }
            else
            {
                OnErrorOccurredEvent?.Invoke(this, new ErrorOccurredEventArgs(result, result.Message));
                return result;
            }
        }

        public virtual async Task<SafeWrapperResult> TryLoadExistingData(CanvasFile canvasFile, BasePastedContentTypeDataModel contentType, CancellationToken cancellationToken)
        {
            SafeWrapperResult result = await InitializeViewModelFromCollectionItem(canvasFile, contentType);

            if (result && CanvasViewModel != null)
            {
                return await CanvasViewModel.TryLoadExistingData(canvasFile, contentType, cancellationToken);
            }
            else
            {
                OnErrorOccurredEvent?.Invoke(this, new ErrorOccurredEventArgs(result, result.Message));
                return result;
            }
        }

        public virtual async Task<SafeWrapperResult> TryDeleteData(bool hideConfirmation = false)
        {
            if (CanvasViewModel == null)
            {
                // The canvas is null, delete the reference file manually
                SafeWrapperResult result = await CanvasHelpers.DeleteCanvasFile(collectionModel, collectionItemViewModel, hideConfirmation);

                if (result != OperationErrorCode.Cancelled && !result)
                {
                    OnErrorOccurredEvent?.Invoke(this, new ErrorOccurredEventArgs(result, result?.Message));
                    return result;
                }
                else if (result != OperationErrorCode.Cancelled)
                {
                    OnFileDeletedEvent?.Invoke(this, new FileDeletedEventArgs(associatedItem));
                }

                return result;
            }
            else
            {
                return await CanvasViewModel.TryDeleteData(hideConfirmation);
            }
        }

        public virtual void DiscardData()
        {
            CanvasViewModel?.DiscardData();
        }

        public virtual async Task<bool> SetDataToClipboard()
        {
            if (CanvasViewModel == null)
            {
                return false;
            }

            return await CanvasViewModel.SetDataToClipboard();
        }

        #endregion

        #region Protected Helpers

        protected virtual async Task<SafeWrapperResult> InitializeViewModelFromCollectionItem(CanvasFile canvasFile, BasePastedContentTypeDataModel contentType)
        {
            // Clear leftover data
            DiscardData();

            // TODO: Add support for adding previews from extensions

            // Decide content type
            contentType = await BasePastedContentTypeDataModel.GetContentType(canvasFile, contentType);

            // Check if contentType is InvalidContentTypeDataModel
            if (contentType is InvalidContentTypeDataModel invalidContentType)
            {
                return invalidContentType.error;
            }

            return InitializeViewModelFromContentType(contentType) ? SafeWrapperResult.S_SUCCESS : new SafeWrapperResult(OperationErrorCode.InvalidOperation, new InvalidOperationException(), "Couldn't display content for this file");
        }

        protected abstract bool InitializeViewModelFromContentType(BasePastedContentTypeDataModel contentType);

        protected bool InitializeViewModelForType<TContentType, TViewModel>(BasePastedContentTypeDataModel contentType, Func<TViewModel> initializer)
            where TContentType : BasePastedContentTypeDataModel
            where TViewModel : TBaseViewModel
        {
            if (contentType is TContentType)
            {
                return InitializeViewModel<TViewModel>(initializer);
            }

            return false;
        }

        protected bool InitializeViewModel<TViewModel>(Func<TViewModel> initializer) 
            where TViewModel : TBaseViewModel
        {
            if (CanvasViewModel is TViewModel) // Reuse View Model
            {
                return true;
            }
            else // Initialize new View Model
            {
                UnhookCanvasViewModelEvents();
                CanvasViewModel = initializer();
                HookCanvasViewModelEvents();

                return true;
            }
        }

        #endregion

        #region Event Handlers

        private void CanvasViewModel_OnContentStartedLoadingEvent(object sender, ContentStartedLoadingEventArgs e)
        {
            RaiseOnContentStartedLoadingEvent(sender, e);
        }

        private void CanvasViewModel_OnContentLoadedEvent(object sender, ContentLoadedEventArgs e)
        {
            RaiseOnContentLoadedEvent(sender, e);
        }

        private void CanvasViewModel_OnFileDeletedEvent(object sender, FileDeletedEventArgs e)
        {
            RaiseOnFileDeletedEvent(sender, e);
        }

        private void CanvasViewModel_OnErrorOccurredEvent(object sender, ErrorOccurredEventArgs e)
        {
            RaiseOnErrorOccurredEvent(sender, e);
        }

        private void CanvasViewModel_OnTipTextUpdateRequestedEvent(object sender, TipTextUpdateRequestedEventArgs e)
        {
            RaiseOnTipTextUpdateRequestedEvent(sender, e);
        }

        private void CanvasViewModel_OnProgressReportedEvent(object sender, ProgressReportedEventArgs e)
        {
            RaiseOnProgressReportedEvent(sender, e);
        }

        #endregion

        #region Event Raisers

        protected void RaiseOnContentStartedLoadingEvent(object s, ContentStartedLoadingEventArgs e) => OnContentStartedLoadingEvent?.Invoke(s, e);

        protected void RaiseOnContentLoadedEvent(object s, ContentLoadedEventArgs e) => OnContentLoadedEvent?.Invoke(s, e);

        protected void RaiseOnFileDeletedEvent(object s, FileDeletedEventArgs e) => OnFileDeletedEvent?.Invoke(s, e);

        protected void RaiseOnErrorOccurredEvent(object s, ErrorOccurredEventArgs e) => OnErrorOccurredEvent?.Invoke(s, e);

        protected void RaiseOnTipTextUpdateRequestedEvent(object s, TipTextUpdateRequestedEventArgs e) => OnTipTextUpdateRequestedEvent?.Invoke(s, e);

        protected void RaiseOnProgressReportedEvent(object s, ProgressReportedEventArgs e) => OnProgressReportedEvent?.Invoke(s, e);

        #endregion

        #region Event Hooks

        protected virtual void HookCanvasViewModelEvents()
        {
            if (CanvasViewModel != null)
            {
                CanvasViewModel.OnContentStartedLoadingEvent += CanvasViewModel_OnContentStartedLoadingEvent;
                CanvasViewModel.OnContentLoadedEvent += CanvasViewModel_OnContentLoadedEvent;
                CanvasViewModel.OnFileDeletedEvent += CanvasViewModel_OnFileDeletedEvent;
                CanvasViewModel.OnErrorOccurredEvent += CanvasViewModel_OnErrorOccurredEvent;
                CanvasViewModel.OnTipTextUpdateRequestedEvent += CanvasViewModel_OnTipTextUpdateRequestedEvent;
                CanvasViewModel.OnProgressReportedEvent += CanvasViewModel_OnProgressReportedEvent;
            }
        }

        protected virtual void UnhookCanvasViewModelEvents()
        {
            if (CanvasViewModel != null)
            {
                CanvasViewModel.OnContentStartedLoadingEvent -= CanvasViewModel_OnContentStartedLoadingEvent;
                CanvasViewModel.OnContentLoadedEvent -= CanvasViewModel_OnContentLoadedEvent;
                CanvasViewModel.OnFileDeletedEvent -= CanvasViewModel_OnFileDeletedEvent;
                CanvasViewModel.OnErrorOccurredEvent -= CanvasViewModel_OnErrorOccurredEvent;
                CanvasViewModel.OnTipTextUpdateRequestedEvent -= CanvasViewModel_OnTipTextUpdateRequestedEvent;
                CanvasViewModel.OnProgressReportedEvent -= CanvasViewModel_OnProgressReportedEvent;
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            UnhookCanvasViewModelEvents();
            CanvasViewModel?.Dispose();
        }

        #endregion
    }
}
