using System;
using ClipboardCanvas.ViewModels;

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
}
