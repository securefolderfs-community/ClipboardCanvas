using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.DataModels.ContentDataModels;
using ClipboardCanvas.EventArguments.InfiniteCanvasEventArgs;
using ClipboardCanvas.ViewModels.UserControls;

namespace ClipboardCanvas.Models
{
    public interface IInteractableCanvasControlModel : IDisposable
    {
        event EventHandler<InfiniteCanvasSaveRequestedEventArgs> OnInfiniteCanvasSaveRequestedEvent;

        Task<InteractableCanvasControlItemViewModel> AddItem(ICollectionModel collectionModel, BaseContentTypeModel contentType, CanvasItem canvasFile, ICanvasItemReceiverModel inifinteCanvasFileReceiver, CancellationToken cancellationToken);

        void RemoveItem(InteractableCanvasControlItemViewModel item);

        bool ContainsItem(InteractableCanvasControlItemViewModel item);

        InteractableCanvasControlItemViewModel FindItem(string path);

        InfiniteCanvasConfigurationModel ConstructConfigurationModel();

        Task RegenerateCanvasPreview();

        void SetConfigurationModel(InfiniteCanvasConfigurationModel canvasConfigurationModel);

        void UpdateItemPositionFromDataPackage(DataPackageView dataPackage, InteractableCanvasControlItemViewModel interactableCanvasControlItem);

        Task ResetAllItemPositions();
    }
}
