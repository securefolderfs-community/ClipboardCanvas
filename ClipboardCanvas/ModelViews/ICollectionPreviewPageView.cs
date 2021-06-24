using ClipboardCanvas.Models;

namespace ClipboardCanvas.ModelViews
{
    public interface ICollectionPreviewPageView
    {
        ICollectionModel AssociatedCollectionModel { get; }
    }
}
