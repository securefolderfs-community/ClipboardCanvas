using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.Contexts.Operations;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.CanavsPasteModels
{
    public class UrlPasteModel : BasePasteModel
    {
        public string Url { get; private set; }

        public UrlPasteModel(ICanvasItemReceiverModel canvasFileReceiver, IOperationContextReceiver operationContextReceiver)
            : base(canvasFileReceiver, operationContextReceiver)
        {
        }

        public override async Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
        {
            if (item is not StorageFile file)
            {
                return ItemIsNotAFileResult;
            }

            SafeWrapper<string> url = await FilesystemOperations.ReadFileText(file);

            this.Url = url;

            return url;
        }

        protected override async Task<SafeWrapper<CanvasItem>> GetCanvasFileFromExtension(ICanvasItemReceiverModel canvasFileReceiver)
        {
            return await canvasFileReceiver.CreateNewCanvasItemFromExtension(Constants.FileSystem.URL_CANVAS_FILE_EXTENSION);
        }

        protected override async Task<SafeWrapperResult> SaveDataToFile()
        {
            return await FilesystemOperations.WriteFileText(await sourceFile, Url);
        }

        protected override async Task<SafeWrapperResult> SetDataFromDataPackage(DataPackageView dataPackage)
        {
            SafeWrapper<string> url = await SafeWrapperRoutines.SafeWrapAsync(
              () => dataPackage.GetTextAsync().AsTask());

            Url = url;

            return url;
        }
    }
}
