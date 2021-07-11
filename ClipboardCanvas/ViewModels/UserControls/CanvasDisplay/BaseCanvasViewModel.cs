using Microsoft.Toolkit.Mvvm.ComponentModel;
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
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.ViewModels.ContextMenu;
using ClipboardCanvas.ViewModels.Dialogs;
using ClipboardCanvas.Services;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public abstract class BaseCanvasViewModel : BaseReadOnlyCanvasViewModel,
        ICanvasPreviewModel,
        ICanvasPreviewEventsModel,
        IDisposable
    {
        #region Protected Members

        /// <summary>
        /// The file that's associated with the canvas, use is not recommended. Use <see cref="associatedFile"/> instead.
        /// </summary>
        //protected StorageFile associatedFile;

        /// <summary>
        /// The source file. If not in reference mode, points to <see cref="associatedFile"/>
        /// </summary>
        //protected IStorageFile sourceFile;

        /// <summary>
        /// Determines whether content is loaded/pasted as reference
        /// </summary>
        //protected bool isContentAsReference;

        #endregion

        #region IPasteCanvasEventsModel

        public event EventHandler<OpenNewCanvasRequestedEventArgs> OnOpenNewCanvasRequestedEvent;

        public event EventHandler<PasteInitiatedEventArgs> OnPasteInitiatedEvent;

        public event EventHandler<FileCreatedEventArgs> OnFileCreatedEvent;

        public event EventHandler<FileModifiedEventArgs> OnFileModifiedEvent;

        public event EventHandler<ProgressReportedEventArgs> OnProgressReportedEvent;

        #endregion

        #region Constructor

        public BaseCanvasViewModel(ISafeWrapperExceptionReporter errorReporter, BasePastedContentTypeDataModel contentType)
            : base(errorReporter, contentType)
        {
        }

        #endregion

        #region IPasteCanvasModel

        public virtual async Task<SafeWrapperResult> TryPasteData(DataPackageView dataPackage, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;

            RaiseOnPasteInitiatedEvent(this, new PasteInitiatedEventArgs(IsContentLoaded, dataPackage));

            SafeWrapperResult result;

            if (IsDisposed)
            {
                return new SafeWrapperResult(OperationErrorCode.InvalidOperation, new ObjectDisposedException(nameof(BaseCanvasViewModel)), "The canvas has been already disposed of.");
            }
            if (IsContentLoaded)
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

            SetContentMode();

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.S_CANCEL;
            }

            result = await TrySetFile();
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
            if (isContentAsReference && sourceItem != null)
            {
                ReferenceFile referenceFile = await ReferenceFile.GetFile(associatedFile);

                // We need to update it since it's empty
                await referenceFile.UpdateReferenceFile(new ReferenceFileData(sourceItem.Path));

                return SafeWrapperResult.S_SUCCESS;
            }
            else
            {
                if (!sourceItem.Path.SequenceEqual(associatedItem.Path)) // Make sure we don't copy to the same path
                {
                    // If pasting a file not raw data from clipboard...

                    // Signify that the file is being pasted
                    RaiseOnTipTextUpdateRequestedEvent(this, new TipTextUpdateRequestedEventArgs("The file is being pasted, please wait.", TimeSpan.FromMilliseconds(Constants.UI.CanvasContent.FILE_PASTING_TIP_DELAY)));

                    // Copy to collection
                    SafeWrapperResult copyResult = await FilesystemOperations.CopyFileAsync(sourceFile, associatedItem as StorageFile, ReportProgress, cancellationToken);
                    
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

            // Get referenced file
            ReferenceFile referenceFile = await ReferenceFile.GetFile(associatedFile);
            IStorageItem referencedItem = referenceFile.ReferencedItem;

            if (referencedItem == null)
            {
                Debugger.Break();
                return ReferencedFileNotFoundResult;
            }

            // Delete reference file
            SafeWrapperResult deletionResult = await FilesystemOperations.DeleteItem(associatedItem, true);
            if (!AssertNoError(deletionResult))
            {
                return deletionResult;
            }

            string fileName = Path.GetFileName(referencedItem.Path);
            SafeWrapper<StorageFile> newFile = await AssociatedCollection.GetOrCreateNewCollectionFile(fileName);

            if (!AssertNoError(newFile))
            {
                return newFile;
            }

            // Copy to the collection
            SafeWrapperResult copyResult = await FilesystemOperations.CopyFileAsync(referencedItem as StorageFile, newFile.Result, ReportProgress, cancellationToken);
            if (!AssertNoError(copyResult))
            {
                // Failed
                Debugger.Break();
                return copyResult;
            }

            associatedItem = newFile.Result;
            sourceItem = associatedItem;
            isContentAsReference = false;
            AssociatedCollection.CurrentCollectionItemViewModel.DangerousUpdateFile(associatedItem);

            if (copyResult)
            {
                RefreshContextMenuItems();
                OnReferencePasted();
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
                // Ignore getting icon
                if (false)
                {
                    var (icon, appName) = await ApplicationHelpers.GetIconFromFileHandlingApp(file as StorageFile, Path.GetExtension(file.Path));
                    if (icon != null && appName != null)
                    {
                        var action_openFile = new SuggestedActionsControlItemViewModel(
                            new AsyncRelayCommand(async () =>
                            {
                                await AssociatedCollection.CurrentCollectionItemViewModel.OpenFile();
                            }), $"Open with {appName}", icon);

                        actions.Add(action_openFile);
                    }
                }
                else
                {
                    var action_openFile = new SuggestedActionsControlItemViewModel(
                        new AsyncRelayCommand(async () =>
                        {
                            await AssociatedCollection.CurrentCollectionItemViewModel.OpenFile();
                        }), "Open file", "\uE8E5");

                    actions.Add(action_openFile);
                }
            }

            // Open directory
            var action_openInFileExplorer = new SuggestedActionsControlItemViewModel(
                new AsyncRelayCommand(async () =>
                {
                    await AssociatedCollection.CurrentCollectionItemViewModel.OpenContainingFolder();
                }), "Open containing folder", "\uE838");

            actions.Add(action_openInFileExplorer);

            return actions;
        }

        public virtual bool SetDataToClipboard(SetClipboardDataSourceType dataSourceSetType)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetStorageItems(new List<IStorageItem>() { sourceItem });

            Clipboard.SetContent(dataPackage);

            return true;
        }

        #endregion

        #region Protected Helpers

        protected virtual void OnCanvasModeChanged(CanvasPreviewMode canvasMode)
        {
        }

        protected virtual void OnReferencePasted()
        {
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

                sourceItem = items.Result.As<IEnumerable<IStorageItem>>().First().As<StorageFile>();

                SafeWrapperResult result = await SetData(sourceFile);

                return result;
            }
            else
            {
                return await SetData(dataPackage);
            }
        }

        protected virtual async Task<SafeWrapperResult> TrySetFile()
        {
            SafeWrapper<StorageFile> file;

            if (isContentAsReference)
            {
                file = await AssociatedCollection.GetOrCreateNewCollectionFileFromExtension(Constants.FileSystem.REFERENCE_FILE_EXTENSION);
            }
            else
            {
                if (sourceItem != null)
                {
                    string fileName = Path.GetFileName(sourceItem.Path);
                    file = await AssociatedCollection.GetOrCreateNewCollectionFile(fileName);
                }
                else
                {
                    file = await TrySetFileWithExtension();
                }
            }

            associatedItem = file.Result;

            if (sourceItem == null)
            {
                sourceItem = associatedItem;
            }

            if (file)
            {
                RaiseOnFileCreatedEvent(this, new FileCreatedEventArgs(contentType, associatedItem));
            }

            return file;
        }

        protected virtual async Task<SafeWrapperResult> SetContentMode(StorageFile file)
        {
            if (ReferenceFile.IsReferenceFile(file))
            {
                // Reference file
                isContentAsReference = true;

                // Get reference file
                ReferenceFile referenceFile = await ReferenceFile.GetFile(file);

                // Set the sourceItem
                sourceItem = referenceFile.ReferencedItem;

                if (sourceItem == null)
                {
                    return ReferencedFileNotFoundResult;
                }
                else
                {
                    return SafeWrapperResult.S_SUCCESS;
                }
            }
            else
            {
                // In collection file
                isContentAsReference = false;

                // Set the sourceItem
                sourceItem = file;

                return SafeWrapperResult.S_SUCCESS;
            }
        }

        protected void SetContentMode()
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
            return sourceItem != null;
        }

        protected abstract Task<SafeWrapperResult> SetData(DataPackageView dataPackage);

        protected abstract Task<SafeWrapper<StorageFile>> TrySetFileWithExtension();

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
