using ClipboardCanvas.Enums;
using ClipboardCanvas.Models;

namespace ClipboardCanvas.DataModels.Navigation
{
    public class HomepageNavigationParameterModel : BaseDisplayFrameParameterDataModel
    {
        public HomepageNavigationParameterModel(CanvasType canvasType, ICollectionModel collectionModel = null)
            : base(collectionModel, canvasType)
        {
        }
    }
}
