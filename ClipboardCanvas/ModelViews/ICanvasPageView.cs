using ClipboardCanvas.Models;

namespace ClipboardCanvas.ModelViews
{
    public interface ICanvasPageView
    {
        ICollectionModel AssociatedCollectionModel { get; }

        ICanvasPreviewModel CanvasPreviewModel { get; }

        void FinishConnectedAnimation();
    }
}
