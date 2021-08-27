using ClipboardCanvas.Models;

namespace ClipboardCanvas.Interfaces.Canvas
{
    public interface ICanGetSourceCanvas<TCanvasViewModel> where TCanvasViewModel : IReadOnlyCanvasPreviewModel
    {
        TCanvasViewModel DangerousGetSourceCanvas();
    }
}
