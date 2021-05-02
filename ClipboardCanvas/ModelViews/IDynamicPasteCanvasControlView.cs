using ClipboardCanvas.Models;

namespace ClipboardCanvas.ModelViews
{
    public interface IDynamicPasteCanvasControlView
    {
        ICollectionsContainerModel CollectionContainer { get; }
    }
}
