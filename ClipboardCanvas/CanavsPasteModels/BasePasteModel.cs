using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

using ClipboardCanvas.Contexts;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ReferenceItems;
using ClipboardCanvas.CanvasFileReceivers;

namespace ClipboardCanvas.CanavsPasteModels
{
    public abstract class BasePasteModel : IPasteModel
    {
        private IStorageItem _pastedItem;

        protected ICanvasFileReceiverModel canvasFileReceiver;

        protected IOperationContext operationContext;

        protected CancellationToken cancellationToken;

        protected CanvasItem canvasFile;

        protected bool isContentAsReference;

        protected IStorageItem associatedItem => canvasFile.AssociatedItem;

        protected StorageFile associatedFile => associatedItem as StorageFile;

        protected Task<IStorageItem> sourceItem => canvasFile.SourceItem;

        protected Task<StorageFile> sourceFile => Task.Run(async () => (await sourceItem) as StorageFile);

        protected readonly SafeWrapperResult ItemIsNotAFileResult = new SafeWrapperResult(OperationErrorCode.InvalidArgument, new ArgumentException(), "The provided item is not a file.");

        public BasePasteModel(ICanvasFileReceiverModel canvasFileReceiver, IOperationContext operationContext)
        {
            this.canvasFileReceiver = canvasFileReceiver;
            this.operationContext = operationContext;
        }

        public async Task<SafeWrapper<CanvasItem>> PasteData(DataPackageView dataPackage, bool pasteAsReference, CancellationToken cancellationToken)
        {
            SafeWrapperResult result;

            this.cancellationToken = cancellationToken;
            this.isContentAsReference = pasteAsReference;

            result = await SetDataFromDataPackageInternal(dataPackage);
            if (!result)
            {
                return (null, result);
            }

            result = await SetCanvasFileInternal();
            if (!result)
            {
                return (null, result);
            }

            result = await SaveDataToFileInternal();
            if (!result)
            {
                return (null, result);
            }

            return (canvasFile, result);
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

        protected async Task<SafeWrapperResult> SetCanvasFileInternal()
        {
            SafeWrapper<CanvasItem> canvasFile;

            if (isContentAsReference && _pastedItem != null)
            {
                canvasFile = await canvasFileReceiver.CreateNewCanvasFileFromExtension(Constants.FileSystem.REFERENCE_FILE_EXTENSION);
            }
            else
            {
                if (_pastedItem != null)
                {
                    string pastedItemFileName = Path.GetFileName(_pastedItem.Path);
                    canvasFile = await canvasFileReceiver.CreateNewCanvasFile(pastedItemFileName);
                }
                else
                {
                    canvasFile = await GetCanvasFileFromExtension(canvasFileReceiver);
                }
            }

            this.canvasFile = canvasFile;
            return canvasFile;
        }

        protected async Task<SafeWrapperResult> SaveDataToFileInternal()
        {
            if (isContentAsReference && await sourceItem == null)
            {
                // We need to update the reference file because the one we created is empty
                ReferenceFile referenceFile = await ReferenceFile.GetFile(associatedFile);
                return await referenceFile.UpdateReferenceFile(new ReferenceFileData(_pastedItem.Path));
            } 
            else
            {
                if (_pastedItem != null && _pastedItem.Path != associatedItem.Path)
                {
                    // Copy the file data directly when pasting a file and not raw data from clipboard
                    return await FilesystemOperations.CopyFileAsync(_pastedItem as StorageFile, associatedFile, operationContext);
                }
                else
                {
                    return await SaveDataToFile();
                }
            }
        }

        protected abstract Task<SafeWrapper<CanvasItem>> GetCanvasFileFromExtension(ICanvasFileReceiverModel canvasFileReceiver);

        protected abstract Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item);

        protected abstract Task<SafeWrapperResult> SetDataFromDataPackage(DataPackageView dataPackage);

        protected abstract Task<SafeWrapperResult> SaveDataToFile();

        public virtual void Dispose()
        {
            // TODO: Should anything be put here?
        }
    }
}
