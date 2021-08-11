using ClipboardCanvas.Enums;
using ClipboardCanvas.Models;

namespace ClipboardCanvas.DataModels.Navigation
{
    public class CollectionPreviewPageNavigationParameterModel : BaseDisplayFrameParameterDataModel
    {
        public CollectionPreviewPageNavigationParameterModel(ICollectionModel collectionModel, CanvasType canvasType)
            : base(collectionModel, canvasType)
        {
        }
    }
}
