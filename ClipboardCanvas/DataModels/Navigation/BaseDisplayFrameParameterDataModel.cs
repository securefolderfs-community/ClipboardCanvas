using ClipboardCanvas.Enums;
using ClipboardCanvas.Models;

namespace ClipboardCanvas.DataModels.Navigation
{
    public abstract class BaseDisplayFrameParameterDataModel
    {
        public readonly ICollectionModel collectionModel;

        public readonly CanvasType canvasType;

        public BaseDisplayFrameParameterDataModel(ICollectionModel collectionModel, CanvasType canvasType)
        {
            this.collectionModel = collectionModel;
            this.canvasType = canvasType;
        }
    }
}
