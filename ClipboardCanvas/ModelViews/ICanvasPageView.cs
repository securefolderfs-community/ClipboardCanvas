using ClipboardCanvas.Models;

namespace ClipboardCanvas.ModelViews
{
    public interface ICanvasPageView
    {
        ICollectionsContainerModel AssociatedCollection { get; }

        IPasteCanvasModel PasteCanvasModel { get; }
    }
}
