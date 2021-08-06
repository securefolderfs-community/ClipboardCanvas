using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using ClipboardCanvas.CanvasFileReceivers;

using ClipboardCanvas.Contexts;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.Filesystem;

namespace ClipboardCanvas.CanavsPasteModels
{
    public class MarkdownPasteModel : BasePasteModel
    {
        private string _text;

        public MarkdownPasteModel(ICanvasFileReceiverModel canvasFileReceiver, IOperationContext operationContext)
            : base(canvasFileReceiver, operationContext)
        {
        }

        protected override async Task<SafeWrapper<CanvasItem>> GetCanvasFileFromExtension(ICanvasFileReceiverModel canvasFileReceiver)
        {
            return await canvasFileReceiver.CreateNewCanvasFileFromExtension(".md");
        }

        protected async override Task<SafeWrapperResult> SaveDataToFile()
        {
            return await FilesystemOperations.WriteFileText(await sourceFile, _text);
        }

        protected override async Task<SafeWrapperResult> SetDataFromDataPackage(DataPackageView dataPackage)
        {
            SafeWrapper<string> text = await SafeWrapperRoutines.SafeWrapAsync(
                          () => dataPackage.GetTextAsync().AsTask());

            _text = text;

            return text;
        }

        protected override async Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
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
