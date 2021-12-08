using System;
using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels;
using Windows.ApplicationModel.DataTransfer;

namespace ClipboardCanvas.EventArguments.CollectionPreview
{
    public class CanvasPreviewOpenRequestedEventArgs : EventArgs
    {
        public readonly CollectionPreviewItemViewModel collectionPreviewItemViewModel;

        public CanvasPreviewOpenRequestedEventArgs(CollectionPreviewItemViewModel collectionPreviewItemViewModel)
        {
            this.collectionPreviewItemViewModel = collectionPreviewItemViewModel;
        }
    }

    public class CanvasPreviewSelectedItemChangedEventArgs : EventArgs
    {
        public readonly CollectionPreviewItemViewModel selectedItem;

        public CanvasPreviewSelectedItemChangedEventArgs(CollectionPreviewItemViewModel selectedItem)
        {
            this.selectedItem = selectedItem;
        }
    }

    public class CanvasPreviewPasteRequestedEventArgs : EventArgs
    {
        public readonly DataPackageView forwardedDataPackage;

        public readonly ICollectionModel collectionModel;

        public CanvasPreviewPasteRequestedEventArgs(DataPackageView forwardedDataPackage, ICollectionModel collectionModel)
        {
            this.forwardedDataPackage = forwardedDataPackage;
            this.collectionModel = collectionModel;
        }
    }
}
