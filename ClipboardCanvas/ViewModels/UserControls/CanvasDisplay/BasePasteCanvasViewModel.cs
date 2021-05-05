using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.Models;
using ClipboardCanvas.Enums;
using Windows.Storage.Streams;
using System.Diagnostics;
using ClipboardCanvas.Helpers.FilesystemHelpers;
using System.Threading;

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

        protected StorageFile associatedFile;

        protected IRandomAccessStream fileStream;

        protected Stream dataStream;

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

        protected abstract ICollectionsContainerModel AssociatedContainer { get; }

        #endregion

        #region IPasteCanvasEventsModel

        public event EventHandler<OpenOpenNewCanvasRequestedEventArgs> OnOpenNewCanvasRequestedEvent;

        public event EventHandler<ContentLoadedEventArgs> OnContentLoadedEvent;

        public event EventHandler<PasteRequestedEventArgs> OnPasteRequestedEvent;

        public event EventHandler<FileCreatedEventArgs> OnFileCreatedEvent;

        public event EventHandler<FileModifiedEventArgs> OnFileModifiedEvent;

        public event EventHandler<FileDeletedEventArgs> OnFileDeletedEvent;

        public event EventHandler<ErrorOccurredEventArgs> OnErrorOccurredEvent;

        #endregion

        #region Constructor

        public BasePasteCanvasViewModel(ISafeWrapperExceptionReporter errorReporter)
        {
            this.errorReporter = errorReporter;
        }

        #endregion

        #region IPasteCanvasModel

        public virtual async Task<SafeWrapperResult> TryLoadExistingData(ICollectionsContainerItemModel itemData, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;

            DiscardData();

            SafeWrapperResult result;
            SafeWrapperResult cancelResult = new SafeWrapperResult(OperationErrorCode.InProgress, "The operation was canceled");

            contentType = itemData.ContentType;
            associatedFile = itemData.File;

            if (!StorageItemHelpers.Exists(associatedFile.Path))
            {
                // We don't invoke OnErrorOccurredEvent here because we want to discard this canvas immediately and not show the error
                return new SafeWrapperResult(OperationErrorCode.NotFound, new FileNotFoundException(), "Canvas not found");
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return cancelResult;
            }

            result = await SetData(associatedFile);
            if (!AssertNoError(result))
            {
                return result;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return cancelResult;
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
            SafeWrapperResult cancelResult = new SafeWrapperResult(OperationErrorCode.InProgress, "The operation was canceled");

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

            result = await SetData(dataPackage);
            if (!AssertNoError(result))
            {
                return result;
            }

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

            result = await TrySaveData();
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

        public virtual async Task<SafeWrapperResult> TrySaveData()
        {
            SafeWrapperResult result;

            result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                using (fileStream = await associatedFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await dataStream.CopyToAsync(fileStream.AsStreamForWrite());
                }
            }, errorReporter);

            if (result)
            {
                RaiseOnFileModifiedEvent(this, new FileModifiedEventArgs(associatedFile, AssociatedContainer));
            }

            return result;
        }

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

        public virtual void DiscardData()
        {
            Dispose();
        }

        public virtual void OpenNewCanvas()
        {
            DiscardData();
            RaiseOnOpenNewCanvasRequestedEvent(this, new OpenOpenNewCanvasRequestedEventArgs());
        }

        #endregion

        #region Protected Helpers

        protected virtual bool AssertNoError(SafeWrapperResult result)
        {
            if (result == null || !result)
            {
                RaiseOnErrorOccurredEvent(this, new ErrorOccurredEventArgs(result, result.Details.message));
                return false;
            }

            return true;
        }

        protected virtual async Task<SafeWrapperResult> SetData(StorageFile file)
        {
            SafeWrapperResult result;

            dataStream = new MemoryStream();
            result = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                using (fileStream = await file?.OpenAsync(FileAccessMode.Read))
                {
                    await fileStream.AsStreamForRead().CopyToAsync(dataStream.AsOutputStream().AsStreamForWrite());
                }
            }, errorReporter);

            return result;
        }

        protected abstract Task<SafeWrapperResult> SetData(DataPackageView dataPackage);

        protected abstract Task<SafeWrapperResult> TrySetFile();

        protected abstract Task<SafeWrapperResult> TryFetchDataToView();

        #region Event Raisers

        protected virtual void RaiseOnOpenNewCanvasRequestedEvent(object s, OpenOpenNewCanvasRequestedEventArgs e) => OnOpenNewCanvasRequestedEvent?.Invoke(s, e);

        protected virtual void RaiseOnContentLoadedEvent(object s, ContentLoadedEventArgs e) => OnContentLoadedEvent?.Invoke(s, e);

        protected virtual void RaiseOnPasteRequestedEvent(object s, PasteRequestedEventArgs e) => OnPasteRequestedEvent?.Invoke(s, e);

        protected virtual void RaiseOnFileCreatedEvent(object s, FileCreatedEventArgs e) => OnFileCreatedEvent?.Invoke(s, e);

        protected virtual void RaiseOnFileModifiedEvent(object s, FileModifiedEventArgs e) => OnFileModifiedEvent?.Invoke(s, e);

        protected virtual void RaiseOnFileDeletedEvent(object s, FileDeletedEventArgs e) => OnFileDeletedEvent?.Invoke(s, e);

        protected virtual void RaiseOnErrorOccurredEvent(object s, ErrorOccurredEventArgs e) => OnErrorOccurredEvent?.Invoke(s, e);

        #endregion

        #endregion

        #region IDisposable

        public virtual void Dispose()
        {
            dataStream?.Dispose();
            fileStream?.Dispose();

            dataStream = null;
            fileStream = null;
            contentType = null;
            associatedFile = null;
        }

        #endregion
    }
}
