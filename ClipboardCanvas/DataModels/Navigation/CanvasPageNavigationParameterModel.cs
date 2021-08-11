using ClipboardCanvas.Enums;
using ClipboardCanvas.Models;

namespace ClipboardCanvas.DataModels.Navigation
{
    public class CanvasPageNavigationParameterModel : BaseDisplayFrameParameterDataModel
    {
        public CanvasPageNavigationParameterModel(ICollectionModel collectionModel, CanvasType canvasType)
            : base(collectionModel, canvasType)
        {
        }
    }
}
