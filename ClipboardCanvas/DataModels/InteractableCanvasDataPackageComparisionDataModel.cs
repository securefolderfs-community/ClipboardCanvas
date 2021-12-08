using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;

namespace ClipboardCanvas.DataModels
{
    public sealed class InteractableCanvasDataPackageComparisionDataModel
    {
        public readonly DataPackageView dataPackage;

        public readonly Point dropPoint;

        public InteractableCanvasDataPackageComparisionDataModel(DataPackageView dataPackage, Point dropPoint)
        {
            this.dataPackage = dataPackage;
            this.dropPoint = dropPoint;
        }
    }
}
