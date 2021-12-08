using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Contexts.Operations;

namespace ClipboardCanvas.CanavsPasteModels
{
    public class MediaPasteModel : BasePasteModel
    {
        public MediaPasteModel(ICanvasItemReceiverModel canvasFileReceiver, IOperationContextReceiver operationContextReceiver)
            : base(canvasFileReceiver, operationContextReceiver)
        {
        }

        protected override Task<SafeWrapper<CanvasItem>> GetCanvasFileFromExtension(ICanvasItemReceiverModel canvasFileReceiver)
        {
            return Task.FromResult<SafeWrapper<CanvasItem>>((null, SafeWrapperResult.SUCCESS));
        }

        protected override Task<SafeWrapperResult> SaveDataToFile()
        {
            return Task.FromResult(SafeWrapperResult.SUCCESS);
        }

        protected override Task<SafeWrapperResult> SetDataFromDataPackage(DataPackageView dataPackage)
        {
            return Task.FromResult(SafeWrapperResult.SUCCESS);
        }

        public override Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
        {
            return Task.FromResult(SafeWrapperResult.SUCCESS);
        }
    }
}
