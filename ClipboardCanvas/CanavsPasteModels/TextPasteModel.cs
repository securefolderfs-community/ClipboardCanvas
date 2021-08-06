using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

using ClipboardCanvas.Contexts;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.Helpers.Filesystem;

namespace ClipboardCanvas.CanavsPasteModels
{
    public class TextPasteModel : BasePasteModel
    {
        private string _text;

        public TextPasteModel(ICanvasFileReceiverModel canvasFileReceiver, IOperationContext operationContext)
            : base(canvasFileReceiver, operationContext)
        {
        }

        protected async override Task<SafeWrapper<CanvasItem>> GetCanvasFileFromExtension()
        {
            return await canvasFileReceiver.CreateNewCanvasFileFromExtension(".txt");
        }

        protected override async Task<SafeWrapperResult> SaveDataToFile()
        {
            return await FilesystemOperations.WriteFileText(await sourceFile, _text);
        }

        protected async override Task<SafeWrapperResult> SetDataFromDataPackage(DataPackageView dataPackage)
        {
            SafeWrapper<string> text = await SafeWrapperRoutines.SafeWrapAsync(
               () => dataPackage.GetTextAsync().AsTask());

            _text = text;

            return text;
        }

        protected async override Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
        {
            if (item is not StorageFile file)
            {
                return ItemIsNotAFileResult;
            }

            SafeWrapper<string> text = await FilesystemOperations.ReadFileText(file);

            this._text = text;

            return text;
        }
    }
}
