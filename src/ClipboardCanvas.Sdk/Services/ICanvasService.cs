using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.ViewModels.Controls.Canvases;
using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.Services
{
    public interface ICanvasService
    {
        Task<BaseCanvasViewModel> GetCanvasForStorableAsync(IStorableChild storable, IDataSourceModel sourceModel, CancellationToken cancellationToken);
    }
}
