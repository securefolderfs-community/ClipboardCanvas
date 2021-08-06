using ClipboardCanvas.Models;

namespace ClipboardCanvas.ModelViews
{
    public interface IInfiniteCanvasControlView
    {
        IInteractableCanvasControlModel InteractableCanvasModel { get; }
    }
}
