using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Toolkit.Mvvm.Input;

using ClipboardCanvas.DataModels.ContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.Models;
using ClipboardCanvas.Enums;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.CanavsPasteModels;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Contexts.Operations;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public abstract class BaseCanvasViewModel : BaseReadOnlyCanvasViewModel, ICanvasPreviewModel, IDisposable
    {
        #region Members

        protected IPasteModel canvasPasteModel;

        #endregion

        #region Events

        public event EventHandler<PasteInitiatedEventArgs> OnPasteInitiatedEvent;

        public event EventHandler<FileCreatedEventArgs> OnFileCreatedEvent;

        public event EventHandler<FileModifiedEventArgs> OnFileModifiedEvent;

        #endregion

        #region Constructor

        public BaseCanvasViewModel(ISafeWrapperExceptionReporter errorReporter, BaseContentTypeModel contentType, IBaseCanvasPreviewControlView view)
            : base(errorReporter, contentType, view)
        {
        }

        #endregion

        #region Canvas Operations

        protected abstract IPasteModel SetCanvasPasteModel();

        public virtual async Task<SafeWrapperResult> TryPasteData(DataPackageView dataPackage, CancellationToken cancellationToken)
        {
            SafeWrapperResult result;

            this.cancellationToken = cancellationToken;

            RaiseOnPasteInitiatedEvent(this, new PasteInitiatedEventArgs(IsContentLoaded, dataPackage, ContentType, associatedCollection));

            if (IsDisposed)
            {
                return new SafeWrapperResult(OperationErrorCode.InvalidOperation, new ObjectDisposedException(nameof(BaseCanvasViewModel)), "The canvas has been already disposed of.");
            }
            else if (IsContentLoaded)
            {
                result = new SafeWrapperResult(OperationErrorCode.AlreadyExists, new InvalidOperationException(), "Content has been already pasted.");
                RaiseOnErrorOccurredEvent(this, new ErrorOccurredEventArgs(result, result.Message, ContentType));

                return result;
            }

            canvasPasteModel = SetCanvasPasteModel();

            if (canvasPasteModel == null)
            {
                return new SafeWrapperResult(OperationErrorCode.AccessUnauthorized, null, "Couldn't paste content.");
            }

            SafeWrapper<CanvasItem> pasteResult = await canvasPasteModel.PasteData(dataPackage, CanPasteAsReference(), cancellationToken);
            this.canvasItem = pasteResult.Result;

            if (!AssertNoError(pasteResult))
            {
                return pasteResult;
            }

            isContentAsReference = canvasPasteModel.IsContentAsReference;

            // Set collectionItemViewModel because it wasn't set before
            this.collectionItemViewModel = associatedCollection.FindCollectionItem(canvasItem);
            
            await OnPasteSucceeded();

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            if (UserSettings.OpenNewCanvasOnPaste)
            {
                OpenNewCanvas();
            }
            else
            {
                // We only need to fetch the data to view if we stay on that canvas
                result = await TryFetchDataToView();
                if (!AssertNoError(result))
                {
                    return result;
                }

                IsContentLoaded = true;
                CanPasteReference = CheckCanPasteReference();

                RefreshContextMenuItems();
                RaiseOnContentLoadedEvent(this, new ContentLoadedEventArgs(ContentType, IsContentLoaded, canvasPasteModel.IsContentAsReference, CanPasteReference));
            }

            return SafeWrapperResult.SUCCESS;
        }

        public virtual async Task<SafeWrapperResult> PasteOverrideReference()
        {
            if (!isContentAsReference)
            {
                return new SafeWrapperResult(OperationErrorCode.InvalidOperation, new InvalidOperationException(), "Cannot paste file that's not a reference");
            }

            SafeWrapper<CanvasItem> newCanvasItemResult = await CanvasHelpers.PasteOverrideReference(canvasItem, associatedCollection, new StatusCenterOperationReceiver());

            if (newCanvasItemResult)
            {
                this.canvasItem = newCanvasItemResult;
                this.collectionItemViewModel = associatedCollection.FindCollectionItem(newCanvasItemResult);

                isContentAsReference = false;

                if (newCanvasItemResult)
                {
                    RefreshContextMenuItems();
                    await OnReferencePasted();
                }
            }

            return newCanvasItemResult;
        }

        public virtual void OpenNewCanvas()
        {
            DiscardData();
            NavigationService.OpenNewCanvas(associatedCollection);
        }

        public virtual async Task<IEnumerable<SuggestedActionsControlItemViewModel>> GetSuggestedActions()
        {
            List<SuggestedActionsControlItemViewModel> actions = new List<SuggestedActionsControlItemViewModel>();

            if (associatedItem == null)
            {
                return actions;
            }

            // Open file
            IStorageFile file = null;
            if (isContentAsReference && await sourceFile != null)
            {
                file = await sourceFile;
            }
            else
            {
                if (associatedFile != null)
                {
                    file = associatedFile;
                }
            }

            // Open file
            var action_openFile = new SuggestedActionsControlItemViewModel(
                                    new AsyncRelayCommand(async () =>
                                    {
                                        await associatedCollection.CurrentCollectionItemViewModel.OpenFile();
                                    }), "Open file", "\uE8E5");

            // Open directory
            var action_openContainingFolder = new SuggestedActionsControlItemViewModel(
                new AsyncRelayCommand(async () =>
                {
                    await associatedCollection.CurrentCollectionItemViewModel.OpenContainingFolder();
                }), "Open containing folder", "\uE838");

            actions.Add(action_openFile);
            actions.Add(action_openContainingFolder);

            return actions;
        }

        #endregion

        #region Protected Helpers

        protected virtual async Task OnPasteSucceeded()
        {
            // Add new item on Timeline
            var todaySection = await TimelineService.GetOrCreateTodaySection();
            await TimelineService.AddItemForSection(todaySection, associatedCollection, collectionItemViewModel);
        }

        protected virtual Task OnReferencePasted()
        {
            return Task.CompletedTask;
        }

        protected virtual bool CanPasteAsReference()
        {
            return UserSettings.AlwaysPasteFilesAsReference;
        }

        #endregion

        #region Event Raisers

        protected void RaiseOnPasteInitiatedEvent(object s, PasteInitiatedEventArgs e) => OnPasteInitiatedEvent?.Invoke(s, e);

        protected void RaiseOnFileCreatedEvent(object s, FileCreatedEventArgs e) => OnFileCreatedEvent?.Invoke(s, e);

        protected void RaiseOnFileModifiedEvent(object s, FileModifiedEventArgs e) => OnFileModifiedEvent?.Invoke(s, e);

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            base.Dispose();

            this.canvasPasteModel?.Dispose();
        }

        #endregion
    }
}
