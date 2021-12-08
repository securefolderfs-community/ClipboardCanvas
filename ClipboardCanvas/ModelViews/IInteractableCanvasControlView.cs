using System.Numerics;
using Windows.Storage.Streams;
using System.Threading.Tasks;

using ClipboardCanvas.ViewModels.UserControls;

namespace ClipboardCanvas.ModelViews
{
    public interface IInteractableCanvasControlView
    {
        Vector2 GetItemPosition(InteractableCanvasControlItemViewModel itemViewModel);

        void SetItemPosition(InteractableCanvasControlItemViewModel itemViewModel, Vector2 position);

        Task<(IBuffer buffer, uint pixelWidth, uint pixelHeight)> GetCanvasImageBuffer();

        void SetOnTop(InteractableCanvasControlItemViewModel itemViewModel);

        int GetCanvasTopIndex(InteractableCanvasControlItemViewModel itemViewModel);

        void SetCanvasTopIndex(InteractableCanvasControlItemViewModel itemViewModel, int topIndex);
    }
}
