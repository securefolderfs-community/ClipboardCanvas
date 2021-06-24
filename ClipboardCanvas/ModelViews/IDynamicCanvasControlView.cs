using ClipboardCanvas.Models;

namespace ClipboardCanvas.ModelViews
{
    public interface IDynamicCanvasControlView
    {
        ICollectionModel CollectionModel { get; }
    }
}
