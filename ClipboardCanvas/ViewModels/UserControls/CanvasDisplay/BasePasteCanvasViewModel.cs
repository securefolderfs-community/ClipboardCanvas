using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

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
using System.Linq;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public abstract class BasePasteCanvasViewModel : ObservableObject,
        IPasteCanvasModel,
        IPasteCanvasEventsModel,
        IDisposable
    {
        #region Protected Members

        protected readonly ISafeWrapperExceptionReporter errorReporter;

        protected CancellationToken cancellationToken;

        protected BasePastedContentTypeDataModel contentType;

        /// <summary>
        /// The file that's associated with the canvas, use is not recommended. Use <see cref="associatedFile"/> instead.
        /// </summary>
        protected StorageFile associatedFile;

        /// <summary>
        /// The source file. If not in reference mode, points to <see cref="associatedFile"/>
        /// </summary>
        protected IStorageFile sourceFile;

        protected IProgress<float> pasteProgress;

        /// <summary>
        /// Determines whether canvas content has been loaded
        /// </summary>
        protected bool isFilled;

        /// <summary>
        /// Determines whether content is loaded/pasted as reference
        /// </summary>
        protected bool contentAsReference;

        #endregion

        #region Protected Properties

        protected SafeWrapperResult CancelledResult => new SafeWrapperResult(OperationErrorCode.Cancelled, "The operation was canceled");

        protected SafeWrapperResult ReferencedFileNotFoundResult => new SafeWrapperResult(OperationErrorCode.NotFound, new FileNotFoundException(), "The file referenced was not found");

        protected abstract ICollectionsContainerModel AssociatedContainer { get; }

        protected bool IsDisposed { get; private set; }

        #endregion

        #region Public Properties

        private CanvasPreviewMode _CanvasMode;
        public CanvasPreviewMode CanvasMode
        {
            get => _CanvasMode;
            protected set
            {
                if (_CanvasMode != value)
                {
                    _CanvasMode = value;
                    OnCanvasModeChanged(_CanvasMode);
                }
            }
        }

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

        #endregion

        #region Constructor

        public BasePasteCanvasViewModel(ISafeWrapperExceptionReporter errorReporter, BasePastedContentTypeDataModel contentType, CanvasPreviewMode canvasMode)
        {
            this.errorReporter = errorReporter;
            this.contentType = contentType;
            this.CanvasMode = canvasMode;
        }

        #endregion

        #region IPasteCanvasModel

        public virtual async Task<SafeWrapperResult> TryLoadExistingData(ICollectionsContainerItemModel itemData, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;

            SafeWrapperResult result;

            contentType = itemData.ContentType;
            associatedFile = itemData.File;

            RaiseOnContentStartedLoadingEvent(this, new ContentStartedLoadingEventArgs(contentType));

            if (!StorageItemHelpers.Exists(associatedFile.Path))
            {
                // We don't invoke OnErrorOccurredEvent here because we want to discard this canvas immediately and not show the error
                return new SafeWrapperResult(OperationErrorCode.NotFound, new FileNotFoundException(), "Canvas not found");
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return CancelledResult;
            }

            result = await SetContentMode(associatedFile);
            if (!AssertNoError(result))
            {
                return result;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return CancelledResult;
            }

            result = await SetData(sourceFile as StorageFile);
            if (!AssertNoError(result))
            {
                return result;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return CancelledResult;
            }

            result = await TryFetchDataToView();
            if (!AssertNoError(result))
            {
                return result;
            }

            isFilled = true;

            RaiseOnContentLoadedEvent(this, new ContentLoadedEventArgs(contentType, isFilled, contentAsReference));

            return result;
        }

        public virtual async Task<SafeWrapperResult> TryPasteData(DataPackageView dataPackage, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;

            RaiseOnPasteRequestedEvent(this, new PasteRequestedEventArgs(isFilled, dataPackage));

            SafeWrapperResult result;
            SafeWrapperResult cancelResult = new SafeWrapperResult(OperationErrorCode.Cancelled, "The operation was canceled");

            if (IsDisposed)
            {
                return null;
            }
            if (isFilled)
            {
                result = new SafeWrapperResult(OperationErrorCode.AlreadyExists, new InvalidOperationException(), "Content has been already pasted.");
                RaiseOnErrorOccurredEvent(this, new ErrorOccurredEventArgs(result, result.Details.message));

                return result;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return cancelResult;
            }

            result = await SetDataInternal(dataPackage);
            if (!AssertNoError(result))
            {
                return result;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return cancelResult;
            }

            SetContentMode();

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return cancelResult;
            }

            result = await TrySetFile();
            if (!AssertNoError(result))
            {
                return result;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return cancelResult;
            }

            result = await TrySaveDataInternal();
            if (!AssertNoError(result))
            {
                return result;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return cancelResult;
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

                isFilled = true;
                RaiseOnContentLoadedEvent(this, new ContentLoadedEventArgs(contentType, isFilled, contentAsReference));
            }

            return result;
        }

        protected virtual async Task<SafeWrapperResult> TrySaveDataInternal()
        {
            if (contentAsReference && sourceFile != null)
            {
                ReferenceFile referenceFile = await ReferenceFile.GetFile(associatedFile);

                // We need to update it since it's empty
                await referenceFile.UpdateReferenceFile(new ReferenceFileData(sourceFile.Path));

                return SafeWrapperResult.S_SUCCESS;
            }
            else
            {
                if (!sourceFile.Path.SequenceEqual(associatedFile.Path)) // Make sure we don't copy to the same path smh
                {
                    // If pasting a file not raw data from clipboard...

                    // Copy to collection
                    SafeWrapperResult copyResult = await FilesystemOperations.CopyFileAsync(sourceFile, associatedFile, ReportProgress, cancellationToken);

                    return copyResult;
                }

                SafeWrapperResult result = await TrySaveData();

                if (result)
                {
                    RaiseOnFileModifiedEvent(this, new FileModifiedEventArgs(associatedFile, AssociatedContainer));
                }

                return result;
            }
        }

        public abstract Task<SafeWrapperResult> TrySaveData();

        public virtual async Task<SafeWrapperResult> TryDeleteData()
        {
            DiscardData();

            SafeWrapperResult result = await SafeWrapperRoutines.SafeWrapAsync(
                () => associatedFile?.DeleteAsync(StorageDeleteOption.PermanentDelete).AsTask(), errorReporter);

            if (result)
            {
                RaiseOnFileDeletedEvent(this, new FileDeletedEventArgs(associatedFile, AssociatedContainer));
            }

            return result;
        }

        public virtual async Task<SafeWrapperResult> PasteOverrideReference()
        {
            if (!contentAsReference)
            {
                return new SafeWrapperResult(OperationErrorCode.InvalidOperation, new InvalidOperationException(), "Cannot paste file that's not a reference");
            }

            // Get referenced file
            ReferenceFile referenceFile = await ReferenceFile.GetFile(associatedFile);
            IStorageFile referencedFile = referenceFile.ReferencedFile;

            if (referencedFile == null)
            {
                Debugger.Break();
                return ReferencedFileNotFoundResult;
            }

            // Delete reference file
            await associatedFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

            string extension = Path.GetExtension(referencedFile.Path);
            string fileName = Path.GetFileNameWithoutExtension(referencedFile.Path);
            SafeWrapper<StorageFile> newFile = await AssociatedContainer.GetEmptyFileToWrite(extension, fileName);

            if (!AssertNoError(newFile))
            {
                return newFile;
            }

            // Copy to the collection
            SafeWrapperResult copyResult = await FilesystemOperations.CopyFileAsync(referencedFile, newFile.Result, ReportProgress, cancellationToken);
            if (!AssertNoError(copyResult))
            {
                // Failed
                Debugger.Break();
                return copyResult;
            }

            associatedFile = newFile;
            sourceFile = associatedFile;
            contentAsReference = false;
            AssociatedContainer.CurrentCanvas.DangerousUpdateFile(associatedFile);

            if (copyResult)
            {
                OnReferencePasted();
            }

            return copyResult;
        }

        public virtual void DiscardData()
        {
            Dispose();
        }

        public virtual void OpenNewCanvas()
        {
            DiscardData();
            RaiseOnOpenNewCanvasRequestedEvent(this, new OpenNewCanvasRequestedEventArgs());
        }

        public virtual async Task<IEnumerable<SuggestedActionsControlItemViewModel>> GetSuggestedActions()
        {
            if (associatedFile == null)
            {
                return null;
            }

            List<SuggestedActionsControlItemViewModel> actions = new List<SuggestedActionsControlItemViewModel>();

            // Open file
            IStorageFile file;
            if (contentAsReference)
            {
                ReferenceFile referenceFile = await ReferenceFile.GetFile(associatedFile);
                file = referenceFile.ReferencedFile;
            }
            else
            {
                file = associatedFile;
            }

            if (file != null)
            {
                if (false)
                {
                    var (icon, appName) = await ApplicationHelpers.GetIconFromFileHandlingApp(file as StorageFile, Path.GetExtension(file.Path));
                    if (icon != null && appName != null)
                    {
                        var action_openFile = new SuggestedActionsControlItemViewModel(
                            async () =>
                            {
                                await AssociatedContainer.CurrentCanvas.OpenFile();
                            }, $"Open with {appName}", icon);

                        actions.Add(action_openFile);
                    }
                }
                else
                {
                    var action_openFile = new SuggestedActionsControlItemViewModel(
                        async () =>
                        {
                            await AssociatedContainer.CurrentCanvas.OpenFile();
                        }, "Open file", "\uE8E5");

                    actions.Add(action_openFile);
                }
            }

            // Open directory
            var action_openInFileExplorer = new SuggestedActionsControlItemViewModel(
                async () =>
                {
                    await AssociatedContainer.CurrentCanvas.OpenContainingFolder();
                }, "Open containing folder", "\uE838");

            actions.Add(action_openInFileExplorer);

            return actions;
        }

        #endregion

        #region Protected Helpers

        protected virtual bool AssertNoError(SafeWrapperResult result)
        {
            if (result == null || !result)
            {
                RaiseOnErrorOccurredEvent(this, new ErrorOccurredEventArgs(result, result?.Details?.message));
                return false;
            }

            return true;
        }

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
            pasteProgress?.Report(value);
            RaiseOnProgressReportedEvent(this, new ProgressReportedEventArgs(value, isIndeterminate, progressType));
        }

        protected virtual async Task<SafeWrapperResult> SetDataInternal(DataPackageView dataPackage)
        {
            if (dataPackage.Contains(StandardDataFormats.StorageItems))
            {
                SafeWrapper<IReadOnlyList<IStorageItem>> items = await SafeWrapperRoutines.SafeWrapAsync(
                    () => dataPackage.GetStorageItemsAsync().AsTask());

                if (!items)
                {
                    Debugger.Break();
                    return (SafeWrapperResult)items;
                }

                sourceFile = items.Result.As<IEnumerable<IStorageItem>>().First().As<StorageFile>();

                SafeWrapperResult result = await SetData(sourceFile as StorageFile);

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

            if (contentAsReference)
            {
                file = await AssociatedContainer.GetEmptyFileToWrite(Constants.FileSystem.REFERENCE_FILE_EXTENSION);
            }
            else
            {
                if (sourceFile != null)
                {
                    string extension = Path.GetExtension(sourceFile.Path);
                    file = await AssociatedContainer.GetEmptyFileToWrite(extension, Path.GetFileNameWithoutExtension(sourceFile.Path));
                }
                else
                {
                    file = await TrySetFileWithExtension();
                }
            }

            associatedFile = file;

            if (sourceFile == null)
            {
                sourceFile = associatedFile;
            }

            if (file)
            {
                RaiseOnFileCreatedEvent(this, new FileCreatedEventArgs(AssociatedContainer, contentType, associatedFile));
            }

            return file;
        }

        protected virtual async Task<SafeWrapperResult> SetContentMode(StorageFile file)
        {
            if (ReferenceFile.IsReferenceFile(file))
            {
                // Reference file
                contentAsReference = true;

                // Get reference file
                ReferenceFile referenceFile = await ReferenceFile.GetFile(file);

                // Set the sourceFile
                sourceFile = referenceFile.ReferencedFile;

                if (sourceFile == null)
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
                contentAsReference = false;

                // Set the sourceFile
                sourceFile = file;

                return SafeWrapperResult.S_SUCCESS;
            }
        }

        protected void SetContentMode()
        {
            if (App.AppSettings.UserSettings.AlwaysPasteFilesAsReference && CanPasteAsReference())
            {
                contentAsReference = true;
            }
            else
            {
                contentAsReference = false;
            }
        }

        protected virtual bool CanPasteAsReference()
        {
            return sourceFile != null;
        }

        protected abstract Task<SafeWrapperResult> SetData(StorageFile file);

        protected abstract Task<SafeWrapperResult> SetData(DataPackageView dataPackage);

        protected abstract Task<SafeWrapper<StorageFile>> TrySetFileWithExtension();

        protected abstract Task<SafeWrapperResult> TryFetchDataToView();

        #region Event Raisers

        protected void RaiseOnOpenNewCanvasRequestedEvent(object s, OpenNewCanvasRequestedEventArgs e) => OnOpenNewCanvasRequestedEvent?.Invoke(s, e);

        protected void RaiseOnContentLoadedEvent(object s, ContentLoadedEventArgs e) => OnContentLoadedEvent?.Invoke(s, e);

        protected void RaiseOnContentStartedLoadingEvent(object s, ContentStartedLoadingEventArgs e) => OnContentStartedLoadingEvent?.Invoke(s, e);

        protected void RaiseOnPasteRequestedEvent(object s, PasteRequestedEventArgs e) => OnPasteRequestedEvent?.Invoke(s, e);

        protected void RaiseOnFileCreatedEvent(object s, FileCreatedEventArgs e) => OnFileCreatedEvent?.Invoke(s, e);

        protected void RaiseOnFileModifiedEvent(object s, FileModifiedEventArgs e) => OnFileModifiedEvent?.Invoke(s, e);

        protected void RaiseOnFileDeletedEvent(object s, FileDeletedEventArgs e) => OnFileDeletedEvent?.Invoke(s, e);

        protected void RaiseOnErrorOccurredEvent(object s, ErrorOccurredEventArgs e) => OnErrorOccurredEvent?.Invoke(s, e);

        protected void RaiseOnProgressReportedEvent(object s, ProgressReportedEventArgs e) => OnProgressReportedEvent?.Invoke(s, e);

        #endregion

        #endregion

        #region IDisposable

        public virtual void Dispose()
        {
            IsDisposed = true;

            associatedFile = null;
            sourceFile = null;
            contentType = null;
        }

        #endregion
    }
}
