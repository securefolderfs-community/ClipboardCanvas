using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ReferenceItems;
using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.Contexts.Operations;
using ClipboardCanvas.CanvasLoadModels;

namespace ClipboardCanvas.CanavsPasteModels
{
    public abstract class BasePasteModel : IPasteModel, ILoadModel
    {
        private IStorageItem _pastedItem;

        protected ICanvasFileReceiverModel canvasFileReceiver;

        protected IOperationContext operationContext;

        protected IOperationContextReceiver operationContextReceiver;

        protected CancellationToken cancellationToken;

        protected CanvasItem canvasItem;

        protected IStorageItem associatedItem => canvasItem.AssociatedItem;

        protected StorageFile associatedFile => associatedItem as StorageFile;

        protected Task<IStorageItem> sourceItem => canvasItem.SourceItem;

        protected Task<StorageFile> sourceFile => Task.Run(async () => (await sourceItem) as StorageFile);

        protected readonly SafeWrapperResult ItemIsNotAFileResult = new SafeWrapperResult(OperationErrorCode.InvalidArgument, new ArgumentException(), "The provided item is not a file.");

        public bool IsContentAsReference { get; protected set; }

        public bool CanPasteReference { get; protected set; }

        #region Events

        public event EventHandler<TipTextUpdateRequestedEventArgs> OnTipTextUpdateRequestedEvent;

        #endregion

        public BasePasteModel(ICanvasFileReceiverModel canvasFileReceiver, IOperationContextReceiver operationContextReceiver)
        {
            this.canvasFileReceiver = canvasFileReceiver;
            this.operationContextReceiver = operationContextReceiver;
        }

        public async Task<SafeWrapper<CanvasItem>> PasteData(DataPackageView dataPackage, bool pasteAsReference, CancellationToken cancellationToken)
        {
            SafeWrapperResult result;

            this.cancellationToken = cancellationToken;
            this.IsContentAsReference = pasteAsReference;

            result = await SetDataFromDataPackageInternal(dataPackage);
            if (!result)
            {
                return (null, result);
            }

            result = await SetCanvasItemInternal();
            if (!result)
            {
                return (null, result);
            }

            result = await SaveDataToFileInternal();
            if (!result)
            {
                return (null, result);
            }

            this.CanPasteReference = CheckCanPasteReference();

            return (canvasItem, result);
        }

        protected async Task<SafeWrapperResult> SetDataFromDataPackageInternal(DataPackageView dataPackage)
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
                    return items;
                }

                this._pastedItem = items.Result.First();

                return await SetDataFromExistingItem(_pastedItem);
            }
            else
            {
                return await SetDataFromDataPackage(dataPackage);
            }
        }

        protected async Task<SafeWrapperResult> SetCanvasItemInternal()
        {
            SafeWrapper<CanvasItem> canvasItem;

            if (IsContentAsReference && _pastedItem != null)
            {
                canvasItem = await canvasFileReceiver.CreateNewCanvasFileFromExtension(Constants.FileSystem.REFERENCE_FILE_EXTENSION);
            }
            else
            {
                if (_pastedItem != null)
                {
                    string pastedItemFileName = Path.GetFileName(_pastedItem.Path);
                    canvasItem = await canvasFileReceiver.CreateNewCanvasFile(pastedItemFileName);
                }
                else
                {
                    canvasItem = await GetCanvasFileFromExtension(canvasFileReceiver);
                }
            }

            this.canvasItem = canvasItem;
            return canvasItem;
        }

        protected async Task<SafeWrapperResult> SaveDataToFileInternal()
        {
            if (IsContentAsReference && await sourceItem == null)
            {
                // We need to update the reference file because the one we created is empty
                ReferenceFile referenceFile = await ReferenceFile.GetFile(associatedFile);
                return await referenceFile.UpdateReference(new ReferenceFileData(_pastedItem.Path));
            } 
            else
            {
                // If pasting a file and not raw data from clipboard...
                if (_pastedItem is StorageFile pastedFile && pastedFile.Path != associatedItem.Path)
                {
                    // Signalize that the file is being pasted
                    RaiseOnTipTextUpdateRequestedEvent(this, new TipTextUpdateRequestedEventArgs("The file is being pasted, please wait.", TimeSpan.FromMilliseconds(Constants.UI.CanvasContent.FILE_PASTING_TIP_DELAY)));

                    this.operationContext = operationContextReceiver.GetOperationContext();

                    // Copy the file data directly when pasting a file and not raw data from clipboard
                    return await FilesystemOperations.CopyFileAsync(pastedFile, associatedFile, operationContext);
                }
                else
                {
                    return await SaveDataToFile();
                }
            }
        }

        protected virtual bool CheckCanPasteReference()
        {
            return true;
        }

        protected abstract Task<SafeWrapper<CanvasItem>> GetCanvasFileFromExtension(ICanvasFileReceiverModel canvasFileReceiver);

        protected abstract Task<SafeWrapperResult> SetDataFromDataPackage(DataPackageView dataPackage);

        protected abstract Task<SafeWrapperResult> SaveDataToFile();

        public abstract Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item);

        #region Event Raisers

        protected void RaiseOnTipTextUpdateRequestedEvent(object s, TipTextUpdateRequestedEventArgs e) => OnTipTextUpdateRequestedEvent?.Invoke(s, e);

        #endregion

        public virtual void Dispose()
        {
            // TODO: Should anything be put here?
        }
    }
}
