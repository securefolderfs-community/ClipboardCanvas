using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.Contexts;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.CanavsPasteModels
{
    public class MediaPasteModel : BasePasteModel
    {
        public MediaPasteModel(ICanvasFileReceiverModel canvasFileReceiver, IOperationContext operationContext)
            : base(canvasFileReceiver, operationContext)
        {
        }

        protected override Task<SafeWrapper<CanvasItem>> GetCanvasFileFromExtension(ICanvasFileReceiverModel canvasFileReceiver)
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

        protected override Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
        {
            return Task.FromResult(SafeWrapperResult.SUCCESS);
        }
    }
}
