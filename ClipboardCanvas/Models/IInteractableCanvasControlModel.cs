using System;
using System.Threading;
using System.Threading.Tasks;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.EventArguments.InfiniteCanvasEventArgs;
using ClipboardCanvas.ViewModels.UserControls;

namespace ClipboardCanvas.Models
{
    public interface IInteractableCanvasControlModel : IDisposable
    {
        event EventHandler<InfiniteCanvasSaveRequestedEventArgs> OnInfiniteCanvasSaveRequestedEvent;

        Task<InteractableCanvasControlItemViewModel> AddItem(ICollectionModel collectionModel, BaseContentTypeModel contentType, CanvasItem canvasFile, CancellationToken cancellationToken);

        void RemoveItem(InteractableCanvasControlItemViewModel item);

        InfiniteCanvasConfigurationModel GetConfigurationModel();

        void SetConfigurationModel(InfiniteCanvasConfigurationModel canvasConfigurationModel);
    }
}
