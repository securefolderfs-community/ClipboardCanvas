using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System.Collections.ObjectModel;
using Microsoft.Toolkit.Uwp;

using ClipboardCanvas.DataModels.ContentDataModels;
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
using ClipboardCanvas.Contexts.Operations;
using ClipboardCanvas.CanvasFileReceivers;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public abstract class BaseReadOnlyCanvasViewModel : ObservableObject, IReadOnlyCanvasPreviewModel, IDisposable
    {
        #region Protected Members

        protected readonly IBaseCanvasPreviewControlView view;

        protected readonly ISafeWrapperExceptionReporter errorReporter;

        protected CancellationToken cancellationToken;

        /// <summary>
        /// Determines whether content is in reference mode
        /// </summary>
        protected bool isContentAsReference;

        protected CollectionItemViewModel collectionItemViewModel;

        protected CanvasItem canvasItem;

        #endregion

        #region Properties

        public static readonly SafeWrapperResult ReferencedFileNotFoundResult = new SafeWrapperResult(OperationErrorCode.NotFound, new FileNotFoundException(), "The file referenced was not found");

        public static readonly SafeWrapperResult ItemIsNotAFileResult = new SafeWrapperResult(OperationErrorCode.InvalidArgument, new ArgumentException(), "The provided item is not a file.");
        
        protected IUserSettingsService UserSettings { get; } = Ioc.Default.GetService<IUserSettingsService>();

        protected ICanvasSettingsService CanvasSettings { get; } = Ioc.Default.GetService<ICanvasSettingsService>();

        protected INavigationService NavigationService { get; } = Ioc.Default.GetService<INavigationService>();

        protected ITimelineService TimelineService { get; } = Ioc.Default.GetService<ITimelineService>();

        protected ICollectionModel AssociatedCollection => view?.CollectionModel;

        /// <inheritdoc cref="AssociatedItem"/>
        protected StorageFile AssociatedFile => AssociatedItem as StorageFile;

        /// <inheritdoc cref="SourceItem"/>
        protected Task<StorageFile> SourceFile => Task.Run(async () => (await SourceItem) as StorageFile);

        /// <inheritdoc cref="SourceItem"/>
        protected Task<StorageFolder> SourceFolder => Task.Run(async () => (await SourceItem) as StorageFolder);

        /// <summary>
        /// The item that's associated with the canvas. For guaranteed true file, use <see cref="SourceItem"/> instead.
        /// </summary>
        protected IStorageItem AssociatedItem => canvasItem?.AssociatedItem;

        /// <summary>
        /// The source item. If not in reference mode, points to <see cref="AssociatedItem"/>
        /// </summary>
        protected Task<IStorageItem> SourceItem => canvasItem?.SourceItem;

        protected bool IsDisposed { get; private set; }

        public BaseContentTypeModel ContentType { get; protected set; }

        public bool IsContentLoaded { get; protected set; }

        public bool CanPasteReference { get; protected set; }

        public ObservableCollection<BaseMenuFlyoutItemViewModel> ContextMenuItems { get; protected set; }

        public ICanvasItemReceiverModel CanvasItemReceiver { get; set; }

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
            this.ContentType = contentType;
            this.view = view;

            this.ContextMenuItems = new ObservableCollection<BaseMenuFlyoutItemViewModel>();
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
            this.ContentType = contentType;
            this.canvasItem = canvasItem;

            RaiseOnContentStartedLoadingEvent(this, new ContentStartedLoadingEventArgs(contentType));

            if (!StorageHelpers.Existsh(AssociatedItem.Path))
            {
                // We don't invoke OnErrorOccurredEvent here because we want to discard this canvas immediately and not show the error
                return new SafeWrapperResult(OperationErrorCode.NotFound, new FileNotFoundException(), "Canvas not found.");
            }
            else if (collectionItemViewModel?.OperationContext.IsOperationStarted ?? false) // Check if it's being pasted
            {
                // Hook event to operation finished event
                if (!collectionItemViewModel.OperationContext.IsEventAlreadyHooked)
                {
                    collectionItemViewModel.OperationContext.OnOperationFinishedEvent += OperationContext_OnOperationFinishedEvent;
                    collectionItemViewModel.OperationContext.IsEventAlreadyHooked = true;
                }
                //this.collectionItemViewModel.OperationContext.OperationProgress = ReportProgress;

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

            result = await SetDataFromExistingItem(await SourceItem);
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

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            IsContentLoaded = true;
            CanPasteReference = CheckCanPasteReference();

            RefreshContextMenuItems();
            RaiseOnContentLoadedEvent(this, new ContentLoadedEventArgs(contentType, IsContentLoaded, isContentAsReference, CanPasteReference));

            return result;
        }

        public virtual async Task<SafeWrapperResult> TryDeleteData(bool hideConfirmation = false)
        {
            SafeWrapperResult result = await CanvasHelpers.DeleteCanvasFile(CanvasItemReceiver ?? AssociatedCollection, canvasItem, hideConfirmation);

            if (result != OperationErrorCode.Canceled && !AssertNoError(result))
            {
                return result;
            }
            else if (result != OperationErrorCode.Canceled)
            {
                RaiseOnFileDeletedEvent(this, new FileDeletedEventArgs(AssociatedItem, AssociatedCollection));
            }

            return result;
        }

        public virtual void DiscardData()
        {
            Dispose();
            IsDisposed = false;
        }

        public virtual async Task<bool> SetDataToDataPackage(DataPackage data)
        {
            data.SetStorageItems((await SourceItem).ToListSingle());

            return true;
        }

        public virtual async Task<bool> CopyData()
        {
            DataPackage data = new DataPackage();
            bool result = await SetDataToDataPackage(data);
            ClipboardHelpers.CopyDataPackage(data);

            return result;
        }

        protected virtual bool CheckCanPasteReference()
        {
            return true;
        }

        protected virtual void RefreshContextMenuItems()
        {
            ContextMenuItems.DisposeClear();

            // Open item
            ContextMenuItems.Add(new MenuFlyoutItemViewModel()
            {
                Command = new AsyncRelayCommand(async () => await StorageHelpers.OpenFile(await SourceItem)),
                IconGlyph = "\uE8E5",
                Text = "OpenFile".GetLocalized()
            });

            // Separator
            ContextMenuItems.Add(new MenuFlyoutSeparatorViewModel());

            // Copy item
            ContextMenuItems.Add(new MenuFlyoutItemViewModel()
            {
                Command = new AsyncRelayCommand(CopyData),
                IconGlyph = "\uE8C8",
                Text = "CopyFile".GetLocalized()
            });

            // Open containing folder
            ContextMenuItems.Add(new MenuFlyoutItemViewModel()
            {
                Command = new AsyncRelayCommand(async () => await StorageHelpers.OpenContainingFolder(await SourceItem)),
                IconGlyph = "\uE838",
                Text = "OpenContainingFolder".GetLocalized()
            });

            // Open reference containing folder
            if (isContentAsReference)
            {
                ContextMenuItems.Add(new MenuFlyoutItemViewModel()
                {
                    Command = new AsyncRelayCommand(() => StorageHelpers.OpenContainingFolder(AssociatedItem)),
                    IconGlyph = "\uE838",
                    Text = "OpenReferenceContainingFolder".GetLocalized()
                });
            }

            // Delete item
            ContextMenuItems.Add(new MenuFlyoutItemViewModel()
            {
                Command = new AsyncRelayCommand(() => TryDeleteData()),
                IconGlyph = "\uE74D",
                Text = isContentAsReference ? "DeleteReference".GetLocalized() : "DeleteFile".GetLocalized(),
            });
        }

        public virtual async Task<SafeWrapper<CanvasItem>> PasteOverrideReference()
        {
            if (!isContentAsReference)
            {
                return new (null, OperationErrorCode.InvalidOperation, new InvalidOperationException(), "Cannot paste file that's not a reference");
            }

            SafeWrapper<CanvasItem> newCanvasItemResult = await CanvasHelpers.PasteOverrideReference(canvasItem, CanvasItemReceiver ?? AssociatedCollection, new StatusCenterOperationReceiver());

            if (newCanvasItemResult)
            {
                this.canvasItem = newCanvasItemResult;
                this.collectionItemViewModel = AssociatedCollection?.FindCollectionItem(newCanvasItemResult);

                isContentAsReference = false;

                if (newCanvasItemResult)
                {
                    RefreshContextMenuItems();
                    await OnReferencePasted();
                }
            }

            return newCanvasItemResult;
        }

        protected virtual Task OnReferencePasted()
        {
            return Task.CompletedTask;
        }

        protected abstract Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item);

        protected abstract Task<SafeWrapperResult> TryFetchDataToView();

        #endregion

        #region Protected Helpers

        /// <summary>
        /// Used in critical functions to check if sub-functions returned SUCCESS.
        /// <br/>
        /// <br/>
        /// This function also calls underlying events to report potential error codes from <paramref name="result"/>.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="errorMessageAutoHide"></param>
        /// <returns></returns>
        protected virtual bool AssertNoError(SafeWrapperResult result, TimeSpan errorMessageAutoHide)
        {
            if (result == null)
            {
                result = SafeWrapperResult.UNKNOWN_FAIL;
            }

            if (!result)
            {
                RaiseOnErrorOccurredEvent(this, new ErrorOccurredEventArgs(result, result.Message, ContentType, errorMessageAutoHide));
                RaiseOnContentLoadFailedEvent(this, new ErrorOccurredEventArgs(result, result.Message, ContentType));
                return false;
            }

            return true;
        }

        /// <inheritdoc cref="AssertNoError(SafeWrapperResult, TimeSpan, bool)"/>
        protected virtual bool AssertNoError(SafeWrapperResult result)
        {
            return AssertNoError(result, TimeSpan.Zero);
        }

        /// <summary>
        /// Wrapper for <see cref="pasteProgress"/> that raises <see cref="OnProgressReportedEvent"/>
        /// </summary>
        protected virtual void ReportProgress(float value)
        {
            RaiseOnProgressReportedEvent(this, new ProgressReportedEventArgs(value, ContentType));
        }

        protected virtual async Task<SafeWrapperResult> SetContentMode()
        {
            if (ReferenceFile.IsReferenceFile(AssociatedFile))
            {
                // Reference file
                isContentAsReference = true;

                // Check if not null
                if (await SourceItem == null)
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
            if (e.result && AssociatedCollection.IsOnOpenedCanvas(collectionItemViewModel))
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
            IsContentLoaded = false;
        }

        #endregion
    }
}
