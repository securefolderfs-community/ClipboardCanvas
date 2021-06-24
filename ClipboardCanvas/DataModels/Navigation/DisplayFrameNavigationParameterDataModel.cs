using ClipboardCanvas.Models;

namespace ClipboardCanvas.DataModels.Navigation
{
    public class DisplayFrameNavigationParameterDataModel
    {
        public readonly ICollectionModel collectionModel;

        public DisplayFrameNavigationParameterDataModel(ICollectionModel collectionModel)
        {
            this.collectionModel = collectionModel;
        }
    }
}
