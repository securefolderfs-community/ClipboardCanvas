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
using ClipboardCanvas.EventArguments.CanvasControl;
using System.Diagnostics;
using ClipboardCanvas.Helpers;
using Windows.ApplicationModel.Core;
using Microsoft.Toolkit.Uwp;
using ClipboardCanvas.ViewModels.ContextMenu;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class DynamicCanvasControlViewModel : ObservableObject, IPasteCanvasModel, IDisposable
    {
        #region Private Members

        private readonly IDynamicCanvasControlView _view;

        #endregion

        #region Public Properties

        private BasePasteCanvasViewModel _CanvasViewModel;

        public BasePasteCanvasViewModel CanvasViewModel
        {
            get => _CanvasViewModel;
            set => SetProperty(ref _CanvasViewModel, value);
        }

        public CanvasPreviewMode CanvasMode => CanvasViewModel?.CanvasMode ?? CanvasPreviewMode.PreviewOnly;

        #endregion

        #region Static Members

        public static CancellationTokenSource CanvasPasteCancellationTokenSource = new CancellationTokenSource();

        #endregion

        #region IPasteCanvasEventsModel

        public event EventHandler<OpenNewCanvasRequestedEventArgs> OnOpenNewCanvasRequestedEvent;

        public event EventHandler<ContentLoadedEventArgs> OnContentLoadedEvent;
        
        public event EventHandler<ContentStartedLoadingEventArgs> OnContentStartedLoadingEvent;

        public event EventHandler<PasteRequestedEventArgs> OnPasteRequestedEvent;

        public event EventHandler<FileCreatedEventArgs> OnFileCreatedEvent;

        public event EventHandler<FileModifiedEventArgs> OnFileModifiedEvent;

        public event EventHandler<FileDeletedEventArgs> OnFileDeletedEvent;

        public event EventHandler<ErrorOccurredEventArgs> OnErrorOccurredEvent;

        public event EventHandler<ProgressReportedEventArgs> OnProgressReportedEvent;
        
        public event EventHandler<TipTextUpdateRequestedEventArgs> OnTipTextUpdateRequestedEvent;

        #endregion

        #region Constructor

        public DynamicCanvasControlViewModel(IDynamicCanvasControlView view)
        {
            this._view = view;
        }

        #endregion

        #region IPasteCanvasModel

        public async Task<SafeWrapperResult> TryLoadExistingData(ICollectionsContainerItemModel itemData, CancellationToken cancellationToken)
        {
            SafeWrapperResult result = await InitializeViewModel(itemData);

            if (result)
            {
                return await CanvasViewModel?.TryLoadExistingData(itemData, cancellationToken);
            }
            else
            {
                OnErrorOccurredEvent?.Invoke(this, new ErrorOccurredEventArgs(result, result.Message));
                return result;
            }
        }

        public async Task<SafeWrapperResult> TryPasteData(DataPackageView dataPackage, CancellationToken cancellationToken)
        {
            SafeWrapperResult result;

            result = await InitializeViewModelAndPaste(dataPackage, cancellationToken);
            
            if (!result)
            {
                OnErrorOccurredEvent?.Invoke(this, new ErrorOccurredEventArgs(result, result.Message));
            }
            
            return result;
        }

        public async Task<SafeWrapperResult> TrySaveData()
        {
            if (CanvasViewModel == null)
            {
                return null;
            }

            return await CanvasViewModel.TrySaveData();
        }

        public async Task<SafeWrapperResult> TryDeleteData()
        {
            if (CanvasViewModel == null)
            {
                return null;
            }

            return await CanvasViewModel.TryDeleteData();
        }

        public async Task<SafeWrapperResult> PasteOverrideReference()
        {
            if (CanvasViewModel == null)
            {
                return null;
            }

            return await CanvasViewModel.PasteOverrideReference();
        }

        public void DiscardData()
        {
            CanvasViewModel?.DiscardData();
        }

        public void OpenNewCanvas()
        {
            CanvasViewModel?.OpenNewCanvas();
        }

        public async Task<IEnumerable<SuggestedActionsControlItemViewModel>> GetSuggestedActions()
        {
            if (CanvasViewModel == null)
            {
                return null;
            }

            return await CanvasViewModel.GetSuggestedActions();
        }

        public async Task<List<BaseMenuFlyoutItemViewModel>> GetContextMenuItems()
        {
            if (CanvasViewModel == null)
            {
                return null;
            }

            return await CanvasViewModel.GetContextMenuItems();
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Decide and initialize View Model from data package
        /// </summary>
        /// <param name="dataPackage"></param>
        /// <returns></returns>
        private async Task<SafeWrapperResult> InitializeViewModelAndPaste(DataPackageView dataPackage, CancellationToken cancellationToken)
        {
            // Decide content type and initialize view model

            // Discard any left over data if we're already pasting to canvas that is filled
            DiscardData();

            // From raw clipboard data
            if (dataPackage.Contains(StandardDataFormats.Bitmap))
            {
                // Image
                InitializeViewModel(() => new ImageCanvasViewModel(_view, CanvasPreviewMode.InteractionAndPreview));
            }
            else if (dataPackage.Contains(StandardDataFormats.Text))
            {
                SafeWrapper<string> text = await SafeWrapperRoutines.SafeWrapAsync(() => dataPackage.GetTextAsync().AsTask());

                if (!text)
                {
                    Debugger.Break(); // What!?
                    return new SafeWrapperResult(OperationErrorCode.AccessUnauthorized, "Couldn't retrieve clipboard data");
                }

                // Check if it's url
                if (StringHelpers.IsUrl(text))
                {
                    // The url may point to file
                    if (StringHelpers.IsUrlFile(text))
                    {
                        // Image
                        InitializeViewModel(() => new ImageCanvasViewModel(_view, CanvasPreviewMode.InteractionAndPreview));
                    }
                    else
                    {
                        // Webpage link
                        //InitializeViewModel(() => new WebViewCanvasViewModel(_view, WebViewCanvasMode.ReadWebsite, CanvasPreviewMode.InteractionAndPreview));
                        if (App.AppSettings.UserSettings.PrioritizeMarkdownOverText)
                        {
                            // Markdown
                            InitializeViewModel(() => new MarkdownCanvasViewModel(_view, CanvasPreviewMode.InteractionAndPreview));
                        }
                        else
                        {
                            // Normal text
                            InitializeViewModel(() => new TextCanvasViewModel(_view, CanvasPreviewMode.InteractionAndPreview));
                        }
                    }
                }
                else
                {
                    if (App.AppSettings.UserSettings.PrioritizeMarkdownOverText)
                    {
                        // Markdown
                        InitializeViewModel(() => new MarkdownCanvasViewModel(_view, CanvasPreviewMode.InteractionAndPreview));
                    }
                    else
                    {
                        // Normal text
                        InitializeViewModel(() => new TextCanvasViewModel(_view, CanvasPreviewMode.InteractionAndPreview));
                    }
                }
            }
            else if (dataPackage.Contains(StandardDataFormats.StorageItems)) // From clipboard storage items
            {
                IReadOnlyList<IStorageItem> items = await dataPackage.GetStorageItemsAsync();

                if (items.Count > 1)
                {
                    // TODO: More than one item, paste in Boundless Canvas
                }
                else if (items.Count == 1)
                {
                    // One item, decide view model for it
                    IStorageItem item = items.First();

                    BasePastedContentTypeDataModel contentType = await BasePastedContentTypeDataModel.GetContentType(item, null);
                    if (contentType is InvalidContentTypeDataModel)
                    {
                        return new SafeWrapperResult(OperationErrorCode.NotFound, "Couldn't get content type for provided data");
                    }

                    InitializeViewModel(contentType);
                }
                else
                {
                    // No items
                    return new SafeWrapperResult(OperationErrorCode.AccessUnauthorized, "Couldn't retrieve clipboard data");
                }
            }

            if (CanvasViewModel == null)
            {
                return new SafeWrapperResult(OperationErrorCode.AccessUnauthorized, "Couldn't retrieve clipboard data");
            }
            else
            {
                OnProgressReportedEvent?.Invoke(this, new ProgressReportedEventArgs(0.0f, true, CanvasPageProgressType.MainCanvasProgressBar));
                SafeWrapperResult result = await CanvasViewModel.TryPasteData(dataPackage, cancellationToken);
                OnProgressReportedEvent?.Invoke(this, new ProgressReportedEventArgs(100.0f, true, CanvasPageProgressType.MainCanvasProgressBar));

                return result;
            }
        }

        /// <summary>
        /// Initialize View Model from existing data
        /// </summary>
        private async Task<SafeWrapperResult> InitializeViewModel(ICollectionsContainerItemModel containerItemModel)
        {
            DiscardData();

            // Decide content type
            BasePastedContentTypeDataModel contentType;

            // TODO: Add support for adding previews from extensions
            contentType = await BasePastedContentTypeDataModel.GetContentType(containerItemModel.File, containerItemModel.ContentType);

            // Check if contentType is InvalidContentTypeDataModel
            if (contentType is InvalidContentTypeDataModel invalidContentType)
            {
                return invalidContentType.error;
            }

            // If containerItemModel.ContentType was null, assign it to reuse it later
            if (containerItemModel.ContentType == null)
            {
                containerItemModel.ContentType = contentType;
            }

            return InitializeViewModel(contentType) ? SafeWrapperResult.S_SUCCESS : new SafeWrapperResult(OperationErrorCode.InvalidOperation, new InvalidOperationException(), "Couldn't display content for this file");
        }

        private bool InitializeViewModel(BasePastedContentTypeDataModel contentType)
        {
            // Initialize View Model

            // Try for image
            if (InitializeViewModelForType<ImageContentType, ImageCanvasViewModel>(contentType, () => new ImageCanvasViewModel(_view, CanvasPreviewMode.InteractionAndPreview)))
            {
                return true;
            }

            // Try for text
            if (InitializeViewModelForType<TextContentType, TextCanvasViewModel>(contentType, () => new TextCanvasViewModel(_view, CanvasPreviewMode.InteractionAndPreview)))
            {
                return true;
            }

            // Try for media
            if (InitializeViewModelForType<MediaContentType, MediaCanvasViewModel>(contentType, () => new MediaCanvasViewModel(_view, CanvasPreviewMode.InteractionAndPreview)))
            {
                return true;
            }

            // Try for WebView
            if (InitializeViewModelForType<WebViewContentType, WebViewCanvasViewModel>(contentType, () => new WebViewCanvasViewModel(_view, (contentType as WebViewContentType)?.mode ?? WebViewCanvasMode.Unknown, CanvasPreviewMode.InteractionAndPreview)))
            {
                return true;
            }

            // Try for markdown
            if (InitializeViewModelForType<MarkdownContentType, MarkdownCanvasViewModel>(contentType, () => new MarkdownCanvasViewModel(_view, CanvasPreviewMode.InteractionAndPreview)))
            {
                return true;
            }

            // Try fallback
            if (InitializeViewModelForType<FallbackContentType, FallbackCanvasViewModel>(contentType, () => new FallbackCanvasViewModel(_view, CanvasPreviewMode.InteractionAndPreview)))
            {
                return true;
            }

            return false;
        }

        private bool InitializeViewModelForType<TContentType, TViewModel>(BasePastedContentTypeDataModel contentType, Func<TViewModel> initializer)
            where TViewModel : BasePasteCanvasViewModel
            where TContentType : BasePastedContentTypeDataModel
        {
            if (contentType is TContentType)
            {
                return InitializeViewModel<TViewModel>(initializer);
            }

            return false;
        }

        public bool InitializeViewModel<TViewModel>(Func<TViewModel> initializer) where TViewModel : BasePasteCanvasViewModel
        {
            if (CanvasViewModel is TViewModel) // Reuse View Model
            {
                return true;
            }
            else // Initialize new View Model
            {
                UnhookEvents();
                CanvasViewModel = initializer();
                HookEvents();

                return true;
            }
        }

        #endregion

        #region Event Handlers

        private void CanvasViewModel_OnTipTextUpdateRequestedEvent(object sender, TipTextUpdateRequestedEventArgs e)
        {
            OnTipTextUpdateRequestedEvent?.Invoke(sender, e);
        }

        private void CanvasViewModel_OnProgressReportedEvent(object sender, ProgressReportedEventArgs e)
        {
            OnProgressReportedEvent?.Invoke(sender, e);
        }

        private void CanvasViewModel_OnErrorOccurredEvent(object sender, ErrorOccurredEventArgs e)
        {
            OnErrorOccurredEvent?.Invoke(sender, e);
        }

        private void CanvasViewModel_OnFileDeletedEvent(object sender, FileDeletedEventArgs e)
        {
            OnFileDeletedEvent?.Invoke(sender, e);
        }

        private void CanvasViewModel_OnFileModifiedEvent(object sender, FileModifiedEventArgs e)
        {
            OnFileModifiedEvent?.Invoke(sender, e);
        }

        private void CanvasViewModel_OnFileCreatedEvent(object sender, FileCreatedEventArgs e)
        {
            OnFileCreatedEvent?.Invoke(sender, e);
        }

        private void CanvasViewModel_OnPasteRequestedEvent(object sender, PasteRequestedEventArgs e)
        {
            OnPasteRequestedEvent?.Invoke(sender, e);
        }

        private void CanvasViewModel_OnContentStartedLoadingEvent(object sender, ContentStartedLoadingEventArgs e)
        {
            OnContentStartedLoadingEvent?.Invoke(sender, e);
        }

        private void CanvasViewModel_OnContentLoadedEvent(object sender, ContentLoadedEventArgs e)
        {
            OnContentLoadedEvent?.Invoke(sender, e);
        }

        private void CanvasViewModel_OnOpenNewCanvasRequestedEvent(object sender, OpenNewCanvasRequestedEventArgs e)
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
                CanvasViewModel.OnOpenNewCanvasRequestedEvent += CanvasViewModel_OnOpenNewCanvasRequestedEvent;
                CanvasViewModel.OnContentLoadedEvent += CanvasViewModel_OnContentLoadedEvent;
                CanvasViewModel.OnContentStartedLoadingEvent += CanvasViewModel_OnContentStartedLoadingEvent;
                CanvasViewModel.OnPasteRequestedEvent += CanvasViewModel_OnPasteRequestedEvent;
                CanvasViewModel.OnFileCreatedEvent += CanvasViewModel_OnFileCreatedEvent;
                CanvasViewModel.OnFileModifiedEvent += CanvasViewModel_OnFileModifiedEvent;
                CanvasViewModel.OnFileDeletedEvent += CanvasViewModel_OnFileDeletedEvent;
                CanvasViewModel.OnErrorOccurredEvent += CanvasViewModel_OnErrorOccurredEvent;
                CanvasViewModel.OnProgressReportedEvent += CanvasViewModel_OnProgressReportedEvent;
                CanvasViewModel.OnTipTextUpdateRequestedEvent += CanvasViewModel_OnTipTextUpdateRequestedEvent;
            }
        }

        private void UnhookEvents()
        {
            if (CanvasViewModel != null)
            {
                CanvasViewModel.OnOpenNewCanvasRequestedEvent -= CanvasViewModel_OnOpenNewCanvasRequestedEvent;
                CanvasViewModel.OnContentLoadedEvent -= CanvasViewModel_OnContentLoadedEvent;
                CanvasViewModel.OnContentStartedLoadingEvent -= CanvasViewModel_OnContentStartedLoadingEvent;
                CanvasViewModel.OnPasteRequestedEvent -= CanvasViewModel_OnPasteRequestedEvent;
                CanvasViewModel.OnFileCreatedEvent -= CanvasViewModel_OnFileCreatedEvent;
                CanvasViewModel.OnFileModifiedEvent -= CanvasViewModel_OnFileModifiedEvent;
                CanvasViewModel.OnFileDeletedEvent -= CanvasViewModel_OnFileDeletedEvent;
                CanvasViewModel.OnErrorOccurredEvent -= CanvasViewModel_OnErrorOccurredEvent;
                CanvasViewModel.OnProgressReportedEvent -= CanvasViewModel_OnProgressReportedEvent;
                CanvasViewModel.OnTipTextUpdateRequestedEvent -= CanvasViewModel_OnTipTextUpdateRequestedEvent;
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            UnhookEvents();
            CanvasViewModel?.Dispose();
        }

        #endregion
    }
}
