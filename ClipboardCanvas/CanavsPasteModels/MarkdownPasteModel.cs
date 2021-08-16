using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using ClipboardCanvas.CanvasFileReceivers;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Contexts.Operations;

namespace ClipboardCanvas.CanavsPasteModels
{
    public class MarkdownPasteModel : BasePasteModel
    {
        public string MarkdownText { get; private set; }

        public MarkdownPasteModel(ICanvasItemReceiverModel canvasFileReceiver, IOperationContextReceiver operationContextReceiver)
            : base(canvasFileReceiver, operationContextReceiver)
        {
        }

        protected override async Task<SafeWrapper<CanvasItem>> GetCanvasFileFromExtension(ICanvasItemReceiverModel canvasFileReceiver)
        {
            return await canvasFileReceiver.CreateNewCanvasItemFromExtension(".md");
        }

        protected async override Task<SafeWrapperResult> SaveDataToFile()
        {
            return await FilesystemOperations.WriteFileText(await sourceFile, MarkdownText);
        }

        protected override async Task<SafeWrapperResult> SetDataFromDataPackage(DataPackageView dataPackage)
        {
            SafeWrapper<string> text = await SafeWrapperRoutines.SafeWrapAsync(
                          () => dataPackage.GetTextAsync().AsTask());

            MarkdownText = text;

            return text;
        }

        public override async Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
        {
            if (item is not StorageFile file)
            {
                return ItemIsNotAFileResult;
            }

            SafeWrapper<string> text = await FilesystemOperations.ReadFileText(file);

            this.MarkdownText = text;

            return text;
        }
    }
}
