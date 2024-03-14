using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.ViewModels.Controls.Canvases;
using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.Services
{
    public interface ICanvasService
    {
        Task<BaseCanvasViewModel> GetViewModelForStorable(IStorableChild storable, ICanvasSourceModel sourceModel, CancellationToken cancellationToken);
    }
}
