using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Enums;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.Models;
using ClipboardCanvas.ReferenceItems;
using ClipboardCanvas.ViewModels.ContextMenu;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Services;
using ClipboardCanvas.ViewModels.UserControls.Collections;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public abstract class BaseReadOnlyCanvasViewModel : ObservableObject, IReadOnlyCanvasPreviewModel, IDisposable
    {
        #region Protected Members

        protected IUserSettingsService UserSettings { get; } = Ioc.Default.GetService<IUserSettingsService>();

        protected ICanvasSettingsService CanvasSettings { get; } = Ioc.Default.GetService<ICanvasSettingsService>();

        protected INavigationService NavigationService { get; } = Ioc.Default.GetService<INavigationService>();

        protected ITimelineService TimelineService { get; } = Ioc.Default.GetService<ITimelineService>();

        protected readonly IBaseCanvasPreviewControlView view;

        protected readonly ISafeWrapperExceptionReporter errorReporter;

        protected CancellationToken cancellationToken;

        protected BaseContentTypeModel contentType;

        /// <inheritdoc cref="associatedItem"/>
        protected StorageFile associatedFile => associatedItem as StorageFile;

        /// <inheritdoc cref="sourceItem"/>
        protected Task<StorageFile> sourceFile => Task.Run(async () => (await sourceItem) as StorageFile);

        /// <inheritdoc cref="sourceItem"/>
        protected Task<StorageFolder> sourceFolder => Task.Run(async () => (await sourceItem) as StorageFolder);

        /// <summary>
        /// The item that's associated with the canvas. For guaranteed true file, use <see cref="sourceItem"/> instead.
        /// </summary>
        protected IStorageItem associatedItem => canvasItem?.AssociatedItem;

        /// <summary>
        /// The source item. If not in reference mode, points to <see cref="associatedItem"/>
        /// </summary>
        protected Task<IStorageItem> sourceItem => canvasItem?.SourceItem;

        /// <summary>
        /// Determines whether content is in reference mode
        /// </summary>
        protected bool isContentAsReference;

        protected ICollectionModel associatedCollection => view?.CollectionModel;

        protected CollectionItemViewModel collectionItemViewModel;

        protected CanvasItem canvasItem;

        #endregion

        #region Protected Properties

        protected bool IsDisposed { get; private set; }

        #endregion

        #region Public Properties

        public static readonly SafeWrapperResult ReferencedFileNotFoundResult = new SafeWrapperResult(OperationErrorCode.NotFound, new FileNotFoundException(), "The file referenced was not found");

        public static readonly SafeWrapperResult ItemIsNotAFileResult = new SafeWrapperResult(OperationErrorCode.InvalidArgument, new ArgumentException(), "The provided item is not a file.");

        public bool IsContentLoaded { get; protected set; }

        public bool CanPasteReference { get; protected set; }

        public List<BaseMenuFlyoutItemViewModel> ContextMenuItems { get; protected set; }

        #endregion

        #region Events

        public event EventHandler<ContentStartedLoadingEventArgs> OnContentStartedLoadingEvent;

        public event EventHandler<ContentLoadedEventArgs> OnContentLoadedEvent;

        public event EventHandler<ErrorOccurredEventArgs> OnContentLoadFailedEvent;

        public event EventHandler<FileDeletedEventArgs> OnFileDeletedEvent;

        public event EventHandler<ErrorOccurredEventArgs> OnErrorOccurredEvent;

        public event EventHandler<TipTextUpdateRequestedEventArgs> OnTipTextUpdateRequestedEvent;

        public event EventHandler<ProgressReportedEventArgs> OnProgressReportedEvent;

        #endregion

        #region Constructor

        public BaseReadOnlyCanvasViewModel(ISafeWrapperExceptionReporter errorReporter, BaseContentTypeModel contentType, IBaseCanvasPreviewControlView view)
        {
            this.errorReporter = errorReporter;
            this.contentType = contentType;
            this.view = view;

            this.ContextMenuItems = new List<BaseMenuFlyoutItemViewModel>();
        }

        #endregion

        #region Canvas Operations

        public virtual async Task<SafeWrapperResult> TryLoadExistingData(CollectionItemViewModel itemData, CancellationToken cancellationToken)
        {
            this.collectionItemViewModel = itemData;

            return await TryLoadExistingData(itemData, itemData.ContentType, cancellationToken);
        }

        public virtual async Task<SafeWrapperResult> TryLoadExistingData(CanvasItem canvasItem, BaseContentTypeModel contentType, CancellationToken cancellationToken)
        {
            SafeWrapperResult result;

            this.cancellationToken = cancellationToken;
            this.contentType = contentType;
            this.canvasItem = canvasItem;

            RaiseOnContentStartedLoadingEvent(this, new ContentStartedLoadingEventArgs(contentType));

            if (!StorageHelpers.Exists(associatedItem.Path))
            {
                // We don't invoke OnErrorOccurredEvent here because we want to discard this canvas immediately and not show the error
                return new SafeWrapperResult(OperationErrorCode.NotFound, new FileNotFoundException(), "Canvas not found.");
            }
            else if (collectionItemViewModel?.OperationContext.IsOperationOngoing ?? false) // Check if it's being pasted
            {
                // Hook event to operation finished event
                if (!collectionItemViewModel.OperationContext.IsEventAlreadyHooked)
                {
                    collectionItemViewModel.OperationContext.OnOperationFinishedEvent += OperationContext_OnOperationFinishedEvent;
                    collectionItemViewModel.OperationContext.IsEventAlreadyHooked = true;
                }
                this.collectionItemViewModel.OperationContext.ProgressDelegate = ReportProgress;

                RaiseOnTipTextUpdateRequestedEvent(this, new TipTextUpdateRequestedEventArgs("The file is being pasted, please wait."));
                return new SafeWrapperResult(OperationErrorCode.InProgress, "Pasting is still in progress");
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            result = await SetContentMode();
            if (!AssertNoError(result))
            {
                return result;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            result = await SetDataFromExistingItem(await sourceItem);
            if (!AssertNoError(result))
            {
                return result;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            result = await TryFetchDataToView();
            if (!AssertNoError(result))
            {
                return result;
            }

            IsContentLoaded = true;
            CanPasteReference = CheckCanPasteReference();

            RefreshContextMenuItems();
            RaiseOnContentLoadedEvent(this, new ContentLoadedEventArgs(contentType, IsContentLoaded, isContentAsReference, CanPasteReference));

            return result;
        }

        public virtual async Task<SafeWrapperResult> TryDeleteData(bool hideConfirmation = false)
        {
            SafeWrapperResult result = await CanvasHelpers.DeleteCanvasFile(associatedCollection, collectionItemViewModel, hideConfirmation);

            if (result != OperationErrorCode.Canceled && !AssertNoError(result))
            {
                return result;
            }
            else if (result != OperationErrorCode.Canceled)
            {
                RaiseOnFileDeletedEvent(this, new FileDeletedEventArgs(associatedItem));
            }

            return result;
        }

        public virtual void DiscardData()
        {
            Dispose();
            IsDisposed = false;
        }

        public virtual async Task<bool> SetDataToClipboard()
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetStorageItems(new List<IStorageItem>() { await sourceItem });

            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();

            return true;
        }

        protected virtual bool CheckCanPasteReference()
        {
            return true;
        }

        protected virtual void RefreshContextMenuItems()
        {
            ContextMenuItems.DisposeClear();

            // May occur when quickly switching between canvases
            if (collectionItemViewModel == null)
            {
                return;
            }

            // Open item
            ContextMenuItems.Add(new MenuFlyoutItemViewModel()
            {
                Command = new AsyncRelayCommand(collectionItemViewModel.OpenFile),
                IconGlyph = "\uE8E5",
                Text = "Open file"
            });

            // Separator
            ContextMenuItems.Add(new MenuFlyoutSeparatorViewModel());

            // Copy item
            ContextMenuItems.Add(new MenuFlyoutItemViewModel()
            {
                Command = new AsyncRelayCommand(SetDataToClipboard),
                IconGlyph = "\uE8C8",
                Text = "Copy file"
            });

            // Open containing folder
            ContextMenuItems.Add(new MenuFlyoutItemViewModel()
            {
                Command = new AsyncRelayCommand(collectionItemViewModel.OpenContainingFolder),
                IconGlyph = "\uE838",
                Text = "Open containing folder"
            });

            // Open reference containing folder
            if (isContentAsReference)
            {
                ContextMenuItems.Add(new MenuFlyoutItemViewModel()
                {
                    Command = new AsyncRelayCommand(() => collectionItemViewModel.OpenContainingFolder(checkForReference: false)),
                    IconGlyph = "\uE838",
                    Text = "Open reference containing folder"
                });
            }

            // Delete item
            ContextMenuItems.Add(new MenuFlyoutItemViewModel()
            {
                Command = new AsyncRelayCommand(() => TryDeleteData()),
                IconGlyph = "\uE74D",
                Text = isContentAsReference ? "Delete reference" : "Delete file"
            });
        }

        protected abstract Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item);

        protected abstract Task<SafeWrapperResult> TryFetchDataToView();

        #endregion

        #region Protected Helpers

        /// <summary>
        /// Used in critical functions to check if sub-functions returned SUCCESS.
        /// <br/>
        /// <br/>
        /// This function also calls underlying events to report result of <paramref name="result"/>.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected bool AssertNoError(SafeWrapperResult result)
        {
            if (result == null)
            {
                result = SafeWrapperResult.UNKNOWN_FAIL;
            }

            if (!result)
            {
                RaiseOnErrorOccurredEvent(this, new ErrorOccurredEventArgs(result, result.Message));
                RaiseOnContentLoadFailedEvent(this, new ErrorOccurredEventArgs(result, result.Message));
                return false;
            }

            return true;
        }

        /// <inheritdoc cref="ReportProgress(float, bool, CanvasPageProgressType)"/>
        protected virtual void ReportProgress(float value)
        {
            ReportProgress(value, false, CanvasPageProgressType.OperationProgressBar);
        }

        /// <summary>
        /// Wrapper for <see cref="pasteProgress"/> that raises <see cref="OnProgressReportedEvent"/>
        /// </summary>
        protected virtual void ReportProgress(float value, bool isIndeterminate, CanvasPageProgressType progressType)
        {
            RaiseOnProgressReportedEvent(this, new ProgressReportedEventArgs(value, isIndeterminate, progressType));
        }

        protected virtual async Task<SafeWrapperResult> SetContentMode()
        {
            if (ReferenceFile.IsReferenceFile(associatedFile))
            {
                // Reference file
                isContentAsReference = true;

                // Check if not null
                if (await sourceItem == null)
                {
                    return ReferencedFileNotFoundResult;
                }
                else
                {
                    return SafeWrapperResult.SUCCESS;
                }
            }
            else
            {
                // Not a reference
                isContentAsReference = false;

                return SafeWrapperResult.SUCCESS;
            }
        }

        #endregion

        #region Event Handlers

        private async void OperationContext_OnOperationFinishedEvent(object sender, OperationFinishedEventArgs e)
        {
            collectionItemViewModel.OperationContext.OnOperationFinishedEvent -= OperationContext_OnOperationFinishedEvent;
            collectionItemViewModel.OperationContext.IsEventAlreadyHooked = false;

            // Load data again when it's finished, and check if we are still on the canvas to load
            if (e.result && associatedCollection.IsOnOpenedCanvas(collectionItemViewModel))
            {
                await TryLoadExistingData(collectionItemViewModel, cancellationToken);
            }
        }

        #endregion

        #region Event Raisers

        protected void RaiseOnContentStartedLoadingEvent(object s, ContentStartedLoadingEventArgs e) => OnContentStartedLoadingEvent?.Invoke(s, e);

        protected void RaiseOnContentLoadedEvent(object s, ContentLoadedEventArgs e) => OnContentLoadedEvent?.Invoke(s, e);

        protected void RaiseOnContentLoadFailedEvent(object s, ErrorOccurredEventArgs e) => OnContentLoadFailedEvent?.Invoke(s, e);

        protected void RaiseOnFileDeletedEvent(object s, FileDeletedEventArgs e) => OnFileDeletedEvent?.Invoke(s, e);

        protected void RaiseOnErrorOccurredEvent(object s, ErrorOccurredEventArgs e) => OnErrorOccurredEvent?.Invoke(s, e);

        protected void RaiseOnTipTextUpdateRequestedEvent(object s, TipTextUpdateRequestedEventArgs e) => OnTipTextUpdateRequestedEvent?.Invoke(s, e);

        protected void RaiseOnProgressReportedEvent(object s, ProgressReportedEventArgs e) => OnProgressReportedEvent?.Invoke(s, e);

        #endregion

        #region IDisposable

        public virtual void Dispose()
        {
            IsDisposed = true;
        }

        #endregion
    }
}
