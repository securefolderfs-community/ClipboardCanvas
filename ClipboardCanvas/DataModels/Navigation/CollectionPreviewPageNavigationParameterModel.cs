using ClipboardCanvas.Enums;
using ClipboardCanvas.Models;

namespace ClipboardCanvas.DataModels.Navigation
{
    public class CollectionPreviewPageNavigationParameterModel : BaseDisplayFrameParameterDataModel
    {
        public readonly CanvasItem itemToSelect;

        public CollectionPreviewPageNavigationParameterModel(ICollectionModel collectionModel, CanvasType canvasType, CanvasItem itemToSelect = null)
            : base(collectionModel, canvasType)
        {
            this.itemToSelect = itemToSelect;
        }
    }
}
