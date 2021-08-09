using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.Contexts;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace ClipboardCanvas.CanavsPasteModels
{
    public class FallbackPasteModel : BasePasteModel
    {
        public FallbackPasteModel(ICanvasFileReceiverModel canvasFileReceiver, IOperationContext operationContext)
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
