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
    public abstract class BaseCanvasViewModel : ObservableObject,
        ICanvasPreviewModel,
        ICanvasPreviewEventsModel,
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

        protected SafeWrapperResult ReferencedFileNotFoundResult => new SafeWrapperResult(OperationErrorCode.NotFound, new FileNotFoundException(), "The file referenced was not found");

        protected ICollectionItemModel AssociatedCollectionItemModel => AssociatedCollection?.CurrentCanvas;

        protected abstract ICollectionModel AssociatedCollection { get; }

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

        public List<BaseMenuFlyoutItemViewModel> ContextMenuItems { get; protected set; }

        #endregion

        #region IPasteCanvasEventsModel

        public event EventHandler<OpenNewCanvasRequestedEventArgs> OnOpenNewCanvasRequestedEvent;

        public event EventHandler<ContentLoadedEventArgs> OnContentLoadedEvent;

        public event EventHandler<ContentStartedLoadingEventArgs> OnContentStartedLoadingEvent;

        public event EventHandler<PasteInitiatedEventArgs> OnPasteInitiatedEvent;

        public event EventHandler<FileCreatedEventArgs> OnFileCreatedEvent;

        public event EventHandler<FileModifiedEventArgs> OnFileModifiedEvent;

        public event EventHandler<FileDeletedEventArgs> OnFileDeletedEvent;

        public event EventHandler<ErrorOccurredEventArgs> OnErrorOccurredEvent;

        public event EventHandler<ProgressReportedEventArgs> OnProgressReportedEvent;

        public event EventHandler<TipTextUpdateRequestedEventArgs> OnTipTextUpdateRequestedEvent;

        #endregion

        #region Constructor

        public BaseCanvasViewModel(ISafeWrapperExceptionReporter errorReporter, BasePastedContentTypeDataModel contentType, CanvasPreviewMode canvasMode)
        {
            this.errorReporter = errorReporter;
            this.contentType = contentType;
            this.CanvasMode = canvasMode;
        }

        #endregion

        #region IPasteCanvasModel

        public virtual async Task<SafeWrapperResult> TryLoadExistingData(ICollectionItemModel itemData, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;

            SafeWrapperResult result;

            contentType = itemData.ContentType;
            associatedFile = itemData.File;

            RaiseOnContentStartedLoadingEvent(this, new ContentStartedLoadingEventArgs(contentType));

            if (!StorageHelpers.Exists(associatedFile.Path))
            {
                // We don't invoke OnErrorOccurredEvent here because we want to discard this canvas immediately and not show the error
                return new SafeWrapperResult(OperationErrorCode.NotFound, new FileNotFoundException(), "Canvas not found.");
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.S_CANCEL;
            }

            result = await SetContentMode(associatedFile);
            if (!AssertNoError(result))
            {
                return result;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.S_CANCEL;
            }

            result = await SetData(sourceFile as StorageFile);
            if (!AssertNoError(result))
            {
                return result;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.S_CANCEL;
            }

            result = await TryFetchDataToView();
            if (!AssertNoError(result))
            {
                return result;
            }

            isFilled = true;

            RefreshContextMenuItems();
            RaiseOnContentLoadedEvent(this, new ContentLoadedEventArgs(contentType, isFilled, contentAsReference));

            return result;
        }

        public virtual async Task<SafeWrapperResult> TryPasteData(DataPackageView dataPackage, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;

            RaiseOnPasteInitiatedEvent(this, new PasteInitiatedEventArgs(isFilled, dataPackage));

            SafeWrapperResult result;

            if (IsDisposed)
            {
                return new SafeWrapperResult(OperationErrorCode.InvalidOperation, new ObjectDisposedException(nameof(BaseCanvasViewModel)), "The canvas has been already disposed of.");
            }
            if (isFilled)
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

                isFilled = true;
                RefreshContextMenuItems();
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
                if (!sourceFile.Path.SequenceEqual(associatedFile.Path)) // Make sure we don't copy to the same path
                {
                    // If pasting a file not raw data from clipboard...

                    // Signify that the file is being pasted
                    RaiseOnTipTextUpdateRequestedEvent(this, new TipTextUpdateRequestedEventArgs("The file is being pasted, please wait.", TimeSpan.FromMilliseconds(Constants.UI.CanvasContent.FILE_PASTING_TIP_DELAY)));

                    // Copy to collection
                    SafeWrapperResult copyResult = await FilesystemOperations.CopyFileAsync(sourceFile, associatedFile, ReportProgress, cancellationToken);
                    
                    return copyResult;
                }

                SafeWrapperResult result = await TrySaveData();

                if (result)
                {
                    RaiseOnFileModifiedEvent(this, new FileModifiedEventArgs(associatedFile));
                }

                return result;
            }
        }

        public abstract Task<SafeWrapperResult> TrySaveData();

        public virtual async Task<SafeWrapperResult> TryDeleteData(bool hideConfirmation = false)
        {
            SafeWrapperResult result = await CanvasHelpers.DeleteCanvasFile(associatedFile, hideConfirmation);

            if (result != OperationErrorCode.Cancelled && !AssertNoError(result))
            {
                return result;
            }
            else if (result != OperationErrorCode.Cancelled)
            {
                AssociatedCollection.RefreshRemoveItem(AssociatedCollectionItemModel);
                RaiseOnFileDeletedEvent(this, new FileDeletedEventArgs(associatedFile));
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
            SafeWrapperResult deletionResult = await FilesystemOperations.DeleteItem(associatedFile, true);
            if (!AssertNoError(deletionResult))
            {
                return deletionResult;
            }

            string extension = Path.GetExtension(referencedFile.Path);
            string fileName = Path.GetFileNameWithoutExtension(referencedFile.Path);
            SafeWrapper<StorageFile> newFile = await AssociatedCollection.GetEmptyFileToWrite(extension, fileName);

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
            AssociatedCollection.CurrentCanvas.DangerousUpdateFile(associatedFile);

            if (copyResult)
            {
                RefreshContextMenuItems();
                OnReferencePasted();
            }

            return copyResult;
        }

        public virtual void DiscardData()
        {
            Dispose();
            IsDisposed = false;
        }

        public virtual void OpenNewCanvas()
        {
            DiscardData();
            RaiseOnOpenNewCanvasRequestedEvent(this, new OpenNewCanvasRequestedEventArgs());
        }

        public virtual async Task<IEnumerable<SuggestedActionsControlItemViewModel>> GetSuggestedActions()
        {
            List<SuggestedActionsControlItemViewModel> actions = new List<SuggestedActionsControlItemViewModel>();

            if (associatedFile == null)
            {
                return actions;
            }

            // Open file
            IStorageFile file;
            if (contentAsReference)
            {
                ReferenceFile referenceFile = await ReferenceFile.GetFile(associatedFile);
                file = referenceFile?.ReferencedFile;
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
                            new AsyncRelayCommand(async () =>
                            {
                                await AssociatedCollection.CurrentCanvas.OpenFile();
                            }), $"Open with {appName}", icon);

                        actions.Add(action_openFile);
                    }
                }
                else
                {
                    var action_openFile = new SuggestedActionsControlItemViewModel(
                        new AsyncRelayCommand(async () =>
                        {
                            await AssociatedCollection.CurrentCanvas.OpenFile();
                        }), "Open file", "\uE8E5");

                    actions.Add(action_openFile);
                }
            }

            // Open directory
            var action_openInFileExplorer = new SuggestedActionsControlItemViewModel(
                new AsyncRelayCommand(async () =>
                {
                    await AssociatedCollection.CurrentCanvas.OpenContainingFolder();
                }), "Open containing folder", "\uE838");

            actions.Add(action_openInFileExplorer);

            return actions;
        }

        public virtual bool SetDataToClipboard(SetClipboardDataSourceType dataSourceSetType)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetStorageItems(new List<IStorageItem>() { sourceFile });

            Clipboard.SetContent(dataPackage);

            return true;
        }

        protected virtual void RefreshContextMenuItems()
        {
            ContextMenuItems.DisposeClear();

            List<BaseMenuFlyoutItemViewModel> items = new List<BaseMenuFlyoutItemViewModel>();

            // May occur when quickly switching between canvases
            if (AssociatedCollection == null)
            {
                ContextMenuItems = items;
            }

            // Open item
            items.Add(new MenuFlyoutItemViewModel()
            {
                Command = new AsyncRelayCommand(AssociatedCollection.CurrentCanvas.OpenFile),
                IconGlyph = "\uE8E5",
                Text = "Open file"
            });

            // Separator
            items.Add(new MenuFlyoutSeparatorViewModel());

            // Copy item
            items.Add(new MenuFlyoutItemViewModel()
            {
                Command = new RelayCommand(() => SetDataToClipboard(SetClipboardDataSourceType.FromContextMenu)),
                IconGlyph = "\uE8C8",
                Text = "Copy file"
            });

            // Open containing folder
            items.Add(new MenuFlyoutItemViewModel()
            {
                Command = new AsyncRelayCommand(AssociatedCollection.CurrentCanvas.OpenContainingFolder),
                IconGlyph = "\uE838",
                Text = "Open containing folder"
            });

            // Open reference containing folder
            if (contentAsReference)
            {
                items.Add(new MenuFlyoutItemViewModel()
                {
                    Command = new AsyncRelayCommand(() => AssociatedCollection.CurrentCanvas.OpenContainingFolder(checkForReference: false)),
                    IconGlyph = "\uE838",
                    Text = "Open reference containing folder"
                });
            }

            // Delete item
            items.Add(new MenuFlyoutItemViewModel()
            {
                Command = new AsyncRelayCommand(() => TryDeleteData()),
                IconGlyph = "\uE74D",
                Text = contentAsReference ? "Delete reference" : "Delete file"
            });

            ContextMenuItems = items;
        }

        #endregion

        #region Protected Helpers

        protected virtual bool AssertNoError(SafeWrapperResult result)
        {
            if (result == null || !result)
            {
                RaiseOnErrorOccurredEvent(this, new ErrorOccurredEventArgs(result, result?.Message));
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
                file = await AssociatedCollection.GetEmptyFileToWrite(Constants.FileSystem.REFERENCE_FILE_EXTENSION);
            }
            else
            {
                if (sourceFile != null)
                {
                    string extension = Path.GetExtension(sourceFile.Path);
                    file = await AssociatedCollection.GetEmptyFileToWrite(extension, Path.GetFileNameWithoutExtension(sourceFile.Path));
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
                RaiseOnFileCreatedEvent(this, new FileCreatedEventArgs(contentType, associatedFile));
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

        #endregion

        #region Event Raisers

        protected void RaiseOnOpenNewCanvasRequestedEvent(object s, OpenNewCanvasRequestedEventArgs e) => OnOpenNewCanvasRequestedEvent?.Invoke(s, e);

        protected void RaiseOnContentLoadedEvent(object s, ContentLoadedEventArgs e) => OnContentLoadedEvent?.Invoke(s, e);

        protected void RaiseOnContentStartedLoadingEvent(object s, ContentStartedLoadingEventArgs e) => OnContentStartedLoadingEvent?.Invoke(s, e);

        protected void RaiseOnPasteInitiatedEvent(object s, PasteInitiatedEventArgs e) => OnPasteInitiatedEvent?.Invoke(s, e);

        protected void RaiseOnFileCreatedEvent(object s, FileCreatedEventArgs e) => OnFileCreatedEvent?.Invoke(s, e);

        protected void RaiseOnFileModifiedEvent(object s, FileModifiedEventArgs e) => OnFileModifiedEvent?.Invoke(s, e);

        protected void RaiseOnFileDeletedEvent(object s, FileDeletedEventArgs e) => OnFileDeletedEvent?.Invoke(s, e);
        
        protected void RaiseOnErrorOccurredEvent(object s, ErrorOccurredEventArgs e) => OnErrorOccurredEvent?.Invoke(s, e);

        protected void RaiseOnProgressReportedEvent(object s, ProgressReportedEventArgs e) => OnProgressReportedEvent?.Invoke(s, e);

        protected void RaiseOnTipTextUpdateRequestedEvent(object s, TipTextUpdateRequestedEventArgs e) => OnTipTextUpdateRequestedEvent?.Invoke(s, e);

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
