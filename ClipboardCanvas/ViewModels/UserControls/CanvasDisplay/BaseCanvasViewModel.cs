using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Toolkit.Mvvm.Input;
using System.Linq;

using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.Models;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.ReferenceItems;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.ModelViews;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public abstract class BaseCanvasViewModel : BaseReadOnlyCanvasViewModel, ICanvasPreviewModel, IDisposable
    {
        #region Private Members

        protected IStorageItem _temporarySourceItem;

        #endregion

        #region Events

        public event EventHandler<OpenNewCanvasRequestedEventArgs> OnOpenNewCanvasRequestedEvent;

        public event EventHandler<PasteInitiatedEventArgs> OnPasteInitiatedEvent;

        public event EventHandler<FileCreatedEventArgs> OnFileCreatedEvent;

        public event EventHandler<FileModifiedEventArgs> OnFileModifiedEvent;

        public event EventHandler<ProgressReportedEventArgs> OnProgressReportedEvent;

        #endregion

        #region Constructor

        public BaseCanvasViewModel(ISafeWrapperExceptionReporter errorReporter, BasePastedContentTypeDataModel contentType, IBaseCanvasPreviewControlView view)
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
                return SafeWrapperResult.S_CANCEL;
            }

            result = await SetDataInternal(dataPackage);
            if (!AssertNoError(result))
            {
                return result;
            }


            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.S_CANCEL;
            }

            SetContentModeForPasting();

            result = await CreateAndSetCanvasFile();
            if (!AssertNoError(result))
            {
                return result;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.S_CANCEL;
            }

            result = await TrySaveDataInternal();
            if (!AssertNoError(result))
            {
                return result;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.S_CANCEL;
            }

            if (App.AppSettings.UserSettings.OpenNewCanvasOnPaste)
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
                await referenceFile.UpdateReferenceFile(new ReferenceFileData(_temporarySourceItem.Path));

                return SafeWrapperResult.S_SUCCESS;
            }
            else
            {
                if ((await sourceItem).Path != associatedItem.Path) // Make sure we don't copy to the same path
                {
                    // If pasting a file not raw data from clipboard...

                    // Signify that the file is being pasted
                    RaiseOnTipTextUpdateRequestedEvent(this, new TipTextUpdateRequestedEventArgs("The file is being pasted, please wait.", TimeSpan.FromMilliseconds(Constants.UI.CanvasContent.FILE_PASTING_TIP_DELAY)));

                    // Copy to collection
                    SafeWrapperResult copyResult = await FilesystemOperations.CopyFileAsync(await sourceFile, associatedFile, ReportProgress, cancellationToken);
                    
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
            SafeWrapper<CollectionItemViewModel> newItemViewModel = await associatedCollection.CreateNewCollectionItemFromFilename(fileName);

            if (!AssertNoError(newItemViewModel))
            {
                return newItemViewModel;
            }

            associatedItemViewModel = newItemViewModel.Result;
            canvasFile = associatedItemViewModel;

            // Copy to the collection
            SafeWrapperResult copyResult = await FilesystemOperations.CopyFileAsync(savedSourceItem as StorageFile, associatedFile, ReportProgress, cancellationToken);
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
            RaiseOnOpenNewCanvasRequestedEvent(this, new OpenNewCanvasRequestedEventArgs());
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
            if (isContentAsReference)
            {
                ReferenceFile referenceFile = await ReferenceFile.GetFile(associatedFile);
                if (referenceFile?.ReferencedItem is StorageFile referencedFile)
                {
                    file = referencedFile;
                }
            }
            else
            {
                if (associatedItem is StorageFile associatedFile)
                {
                    file = associatedFile;
                }
            }

            if (file != null)
            {
                //// Ignore getting icon
                //if (false)
                //{
                //    var (icon, appName) = await ApplicationHelpers.GetIconFromFileHandlingApp(file as StorageFile, Path.GetExtension(file.Path));
                //    if (icon != null && appName != null)
                //    {
                //        var action_openFile = new SuggestedActionsControlItemViewModel(
                //            new AsyncRelayCommand(async () =>
                //            {
                //                await associatedCollection.CurrentCollectionItemViewModel.OpenFile();
                //            }), $"Open with {appName}", icon);

                //        actions.Add(action_openFile);
                //    }
                //}
                //else
                {
                    var action_openFile = new SuggestedActionsControlItemViewModel(
                        new AsyncRelayCommand(async () =>
                        {
                            await associatedCollection.CurrentCollectionItemViewModel.OpenFile();
                        }), "Open file", "\uE8E5");

                    actions.Add(action_openFile);
                }
            }

            // Open directory
            var action_openInFileExplorer = new SuggestedActionsControlItemViewModel(
                new AsyncRelayCommand(async () =>
                {
                    await associatedCollection.CurrentCollectionItemViewModel.OpenContainingFolder();
                }), "Open containing folder", "\uE838");

            actions.Add(action_openInFileExplorer);

            return actions;
        }

        #endregion

        #region Protected Helpers

        protected virtual async Task OnReferencePasted()
        {
            await Task.CompletedTask;
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

        protected virtual async Task<SafeWrapperResult> CreateAndSetCanvasFile()
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
                    itemViewModel = await associatedCollection.CreateNewCollectionItemFromFilename(fileName);
                }
                else
                {
                    itemViewModel = await TrySetFileWithExtension();
                }
            }

            this.associatedItemViewModel = itemViewModel;
            this.canvasFile = associatedItemViewModel;

            if (itemViewModel)
            {
                RaiseOnFileCreatedEvent(this, new FileCreatedEventArgs(contentType, associatedItem));
            }

            return itemViewModel;
        }

        protected void SetContentModeForPasting()
        {
            if (App.AppSettings.UserSettings.AlwaysPasteFilesAsReference && CanPasteAsReference())
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
            return App.AppSettings.UserSettings.AlwaysPasteFilesAsReference;
        }

        protected abstract Task<SafeWrapperResult> SetData(DataPackageView dataPackage);

        protected abstract Task<SafeWrapper<CollectionItemViewModel>> TrySetFileWithExtension();

        #endregion

        #region Event Raisers

        protected void RaiseOnOpenNewCanvasRequestedEvent(object s, OpenNewCanvasRequestedEventArgs e) => OnOpenNewCanvasRequestedEvent?.Invoke(s, e);

        protected void RaiseOnPasteInitiatedEvent(object s, PasteInitiatedEventArgs e) => OnPasteInitiatedEvent?.Invoke(s, e);

        protected void RaiseOnFileCreatedEvent(object s, FileCreatedEventArgs e) => OnFileCreatedEvent?.Invoke(s, e);

        protected void RaiseOnFileModifiedEvent(object s, FileModifiedEventArgs e) => OnFileModifiedEvent?.Invoke(s, e);
        
        protected void RaiseOnProgressReportedEvent(object s, ProgressReportedEventArgs e) => OnProgressReportedEvent?.Invoke(s, e);

        #endregion
    }
}
