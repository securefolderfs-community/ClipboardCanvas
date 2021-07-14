using ClipboardCanvas.Enums;
using ClipboardCanvas.Models;

namespace ClipboardCanvas.DataModels.Navigation
{
    public class DisplayFrameNavigationParameterDataModel
    {
        public readonly ICollectionModel collectionModel;

        public readonly CanvasType canvasType;

        public DisplayFrameNavigationParameterDataModel(ICollectionModel collectionModel, CanvasType canvasType)
        {
            this.collectionModel = collectionModel;
            this.canvasType = canvasType;
        }
    }
}
