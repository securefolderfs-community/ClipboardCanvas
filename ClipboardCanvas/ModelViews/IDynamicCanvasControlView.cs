using ClipboardCanvas.Models;

namespace ClipboardCanvas.ModelViews
{
    public interface IDynamicCanvasControlView
    {
        ICollectionsContainerModel CollectionContainer { get; }
    }
}
