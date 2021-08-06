using ClipboardCanvas.Enums;
using ClipboardCanvas.Models;

namespace ClipboardCanvas.DataModels.Navigation
{
    public class DisplayFrameParameterDataModel
    {
        public readonly ICollectionModel collectionModel;

        public readonly CanvasType canvasType;

        public DisplayFrameParameterDataModel(ICollectionModel collectionModel, CanvasType canvasType)
        {
            this.collectionModel = collectionModel;
            this.canvasType = canvasType;
        }
    }
}
