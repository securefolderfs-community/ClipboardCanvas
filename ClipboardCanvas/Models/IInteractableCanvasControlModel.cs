using System.Threading;
using System.Threading.Tasks;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.ViewModels.UserControls;

namespace ClipboardCanvas.Models
{
    public interface IInteractableCanvasControlModel
    {
        Task<InteractableCanvasControlItemViewModel> AddItem(ICollectionModel collectionModel, BaseContentTypeModel contentType, CanvasItem canvasFile, CancellationToken cancellationToken);
    }
}
