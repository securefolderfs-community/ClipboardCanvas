using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.Contexts.Operations;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.Filesystem;

namespace ClipboardCanvas.CanavsPasteModels
{
    public class WebViewPasteModel : BasePasteModel
    {
        private readonly WebViewCanvasMode _mode;

        public string HtmlText { get; private set; }

        public string Source { get; private set; }

        public WebViewPasteModel(WebViewCanvasMode mode, ICanvasItemReceiverModel canvasFileReceiver, IOperationContextReceiver operationContextReceiver)
            : base(canvasFileReceiver, operationContextReceiver)
        {
            this._mode = mode;
        }

        public override async Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
        {
            if (item is not StorageFile file)
            {
                return ItemIsNotAFileResult;
            }

            SafeWrapper<string> text = await FilesystemOperations.ReadFileText(file);

            if (_mode == WebViewCanvasMode.ReadWebsite)
            {
                Source = text;
            }
            else
            {
                HtmlText = text;
            }
            
            return text;
        }

        protected override async Task<SafeWrapper<CanvasItem>> GetCanvasFileFromExtension(ICanvasItemReceiverModel canvasFileReceiver)
        {
            if (_mode == WebViewCanvasMode.ReadWebsite)
            {
                return await canvasFileReceiver.CreateNewCanvasItemFromExtension(Constants.FileSystem.WEBSITE_LINK_FILE_EXTENSION);
            }
            else
            {
                return await canvasFileReceiver.CreateNewCanvasItemFromExtension(".html");
            }
        }

        protected override async Task<SafeWrapperResult> SaveDataToFile()
        {
            SafeWrapperResult result;

            if (associatedFile == null)
            {
                return ItemIsNotAFileResult;
            }

            if (_mode == WebViewCanvasMode.ReadWebsite)
            {
                result = await FilesystemOperations.WriteFileText(associatedFile, Source);
            }
            else
            {
                result = await FilesystemOperations.WriteFileText(associatedFile, HtmlText);
            }

            return result;
        }

        protected override async Task<SafeWrapperResult> SetDataFromDataPackage(DataPackageView dataPackage)
        {
            SafeWrapper<string> text = await SafeWrapperRoutines.SafeWrapAsync(() =>
                dataPackage.GetTextAsync().AsTask());

            if (_mode == WebViewCanvasMode.ReadWebsite)
            {
                Source = text;
            }
            else
            {
                HtmlText = text;
            }

            return text;
        }
    }
}
