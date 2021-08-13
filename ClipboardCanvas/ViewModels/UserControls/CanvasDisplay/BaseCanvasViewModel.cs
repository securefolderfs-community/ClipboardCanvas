using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.ApplicationModel.Core;
using Microsoft.Toolkit.Uwp;
using System.Linq;

using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.Models;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.ReferenceItems;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.CanavsPasteModels;
using ClipboardCanvas.ViewModels.UserControls.Collections;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public abstract class BaseCanvasViewModel : BaseReadOnlyCanvasViewModel, ICanvasPreviewModel, IDisposable
    {
        #region Members

        protected IStorageItem _temporarySourceItem;

        #endregion

        #region Properties

        protected abstract IPasteModel CanvasPasteModel { get; }

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

        public virtual async Task<SafeWrapperResult> TryPasteData(DataPackageView dataPackage, CancellationToken cancellationToken)
        {
            SafeWrapperResult result;

            this.cancellationToken = cancellationToken;

            RaiseOnPasteInitiatedEvent(this, new PasteInitiatedEventArgs(IsContentLoaded, dataPackage));

            if (IsDisposed)
            {
                return new SafeWrapperResult(OperationErrorCode.InvalidOperation, new ObjectDisposedException(nameof(BaseCanvasViewModel)), "The canvas has been already disposed of.");
            }
            else if (IsContentLoaded)
            {
                result = new SafeWrapperResult(OperationErrorCode.AlreadyExists, new InvalidOperationException(), "Content has been already pasted.");
                RaiseOnErrorOccurredEvent(this, new ErrorOccurredEventArgs(result, result.Message));

                return result;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            result = await SetDataInternal(dataPackage);
            if (!AssertNoError(result))
            {
                return result;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            SetContentModeForPasting();

            result = await CreateAndSetCanvasItem();
            if (!AssertNoError(result))
            {
                return result;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            result = await TrySaveDataInternal();
            if (!AssertNoError(result))
            {
                return result;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            OnPasteSucceeded();

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
                RefreshContextMenuItems();
                RaiseOnContentLoadedEvent(this, new ContentLoadedEventArgs(contentType, IsContentLoaded, isContentAsReference));
            }

            return result;
        }

        protected virtual async Task<SafeWrapperResult> TrySaveDataInternal()
        {
            if (isContentAsReference && await sourceItem == null)
            {
                ReferenceFile referenceFile = await ReferenceFile.GetFile(associatedFile);

                // We need to update it since it's empty
                return await referenceFile.UpdateReferenceFile(new ReferenceFileData(_temporarySourceItem.Path));
            }
            else
            {
                if (_temporarySourceItem != null && _temporarySourceItem.Path != associatedItem.Path) // Make sure we don't copy to the same path
                {
                    // If pasting a file not raw data from clipboard...

                    // Signalize that the file is being pasted
                    RaiseOnTipTextUpdateRequestedEvent(this, new TipTextUpdateRequestedEventArgs("The file is being pasted, please wait.", TimeSpan.FromMilliseconds(Constants.UI.CanvasContent.FILE_PASTING_TIP_DELAY)));

                    SafeWrapperResult copyResult = SafeWrapperResult.UNKNOWN_FAIL;

                    // Copy to collection
                    this.associatedItemViewModel.OperationContext.CancellationToken = cancellationToken;
                    this.associatedItemViewModel.OperationContext.ProgressDelegate = ReportProgress;

                    await CoreApplication.MainView.DispatcherQueue.EnqueueAsync(async () =>
                    {
                        copyResult = await FilesystemOperations.CopyFileAsync(_temporarySourceItem as StorageFile, associatedFile, associatedItemViewModel.OperationContext);
                    });

                    return copyResult;
                }

                SafeWrapperResult result = await TrySaveData();

                if (result)
                {
                    RaiseOnFileModifiedEvent(this, new FileModifiedEventArgs(associatedItem));
                }

                return result;
            }
        }

        public abstract Task<SafeWrapperResult> TrySaveData();

        public virtual async Task<SafeWrapperResult> PasteOverrideReference()
        {
            if (!isContentAsReference)
            {
                return new SafeWrapperResult(OperationErrorCode.InvalidOperation, new InvalidOperationException(), "Cannot paste file that's not a reference");
            }

            // Check referenced file
            if (await sourceFile == null)
            {
                Debugger.Break();
                return ReferencedFileNotFoundResult;
            }

            IStorageItem savedSourceItem = await sourceItem;

            // Delete reference file
            SafeWrapperResult deletionResult = await associatedCollection.DeleteCollectionItem(associatedItemViewModel, true);
            if (!AssertNoError(deletionResult))
            {
                return deletionResult;
            }

            string fileName = Path.GetFileName((await sourceFile).Path);
            SafeWrapper<CollectionItemViewModel> newItemViewModel = await associatedCollection.CreateNewCollectionItem(fileName);

            if (!AssertNoError(newItemViewModel))
            {
                return newItemViewModel;
            }

            associatedItemViewModel = newItemViewModel.Result;
            canvasItem = associatedItemViewModel;

            // Copy to the collection
            this.associatedItemViewModel.OperationContext.CancellationToken = cancellationToken;
            this.associatedItemViewModel.OperationContext.ProgressDelegate = ReportProgress;

            SafeWrapperResult copyResult = SafeWrapperResult.UNKNOWN_FAIL;
            await CoreApplication.MainView.DispatcherQueue.EnqueueAsync(async () =>
            {
                copyResult = await FilesystemOperations.CopyFileAsync(savedSourceItem as StorageFile, associatedFile, associatedItemViewModel.OperationContext);
            });

            if (!AssertNoError(copyResult))
            {
                // Failed
                Debugger.Break();
                return copyResult;
            }

            isContentAsReference = false;

            if (copyResult)
            {
                RefreshContextMenuItems();
                await OnReferencePasted();
            }

            return copyResult;
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

        protected virtual void OnPasteSucceeded()
        {
            // Add new item on Timeline
            var todaySection = TimelineService.GetOrCreateTodaySection();
            TimelineService.AddItemForSection(todaySection, associatedCollection, associatedItemViewModel);
        }

        protected virtual Task OnReferencePasted()
        {
            return Task.CompletedTask;
        }

        protected virtual async Task<SafeWrapperResult> SetDataInternal(DataPackageView dataPackage)
        {
            if (dataPackage.Contains(StandardDataFormats.StorageItems)
                && !dataPackage.Contains(StandardDataFormats.ApplicationLink)
                && !dataPackage.Contains(StandardDataFormats.Bitmap)
                && !dataPackage.Contains(StandardDataFormats.Html)
                && !dataPackage.Contains(StandardDataFormats.Rtf)
                && !dataPackage.Contains(StandardDataFormats.Text)
                && !dataPackage.Contains(StandardDataFormats.UserActivityJsonArray)
                && !dataPackage.Contains(StandardDataFormats.WebLink))
            {
                SafeWrapper<IReadOnlyList<IStorageItem>> items = await SafeWrapperRoutines.SafeWrapAsync(
                    () => dataPackage.GetStorageItemsAsync().AsTask());

                if (!items)
                {
                    Debugger.Break();
                    return (SafeWrapperResult)items;
                }

                _temporarySourceItem = items.Result.First();

                SafeWrapperResult result = await SetDataFromExistingFile(_temporarySourceItem);

                return result;
            }
            else
            {
                return await SetData(dataPackage);
            }
        }

        protected virtual async Task<SafeWrapperResult> CreateAndSetCanvasItem()
        {
            SafeWrapper<CollectionItemViewModel> itemViewModel;

            if (isContentAsReference)
            {
                itemViewModel = await associatedCollection.CreateNewCollectionItemFromExtension(Constants.FileSystem.REFERENCE_FILE_EXTENSION);
            }
            else
            {
                if (_temporarySourceItem != null)
                {
                    string fileName = Path.GetFileName(_temporarySourceItem.Path);
                    itemViewModel = await associatedCollection.CreateNewCollectionItem(fileName);
                }
                else
                {
                    itemViewModel = await TrySetFileWithExtension();
                }
            }

            this.associatedItemViewModel = itemViewModel;
            this.canvasItem = associatedItemViewModel;

            if (itemViewModel)
            {
                RaiseOnFileCreatedEvent(this, new FileCreatedEventArgs(contentType, associatedItem));
            }

            return itemViewModel;
        }

        protected void SetContentModeForPasting()
        {
            if (UserSettings.AlwaysPasteFilesAsReference && CanPasteAsReference())
            {
                isContentAsReference = true;
            }
            else
            {
                isContentAsReference = false;
            }
        }

        protected virtual bool CanPasteAsReference()
        {
            return _temporarySourceItem != null;
        }

        protected abstract Task<SafeWrapperResult> SetData(DataPackageView dataPackage);

        protected abstract Task<SafeWrapper<CollectionItemViewModel>> TrySetFileWithExtension();

        #endregion

        #region Event Raisers

        protected void RaiseOnPasteInitiatedEvent(object s, PasteInitiatedEventArgs e) => OnPasteInitiatedEvent?.Invoke(s, e);

        protected void RaiseOnFileCreatedEvent(object s, FileCreatedEventArgs e) => OnFileCreatedEvent?.Invoke(s, e);

        protected void RaiseOnFileModifiedEvent(object s, FileModifiedEventArgs e) => OnFileModifiedEvent?.Invoke(s, e);
        
        #endregion
    }
}
