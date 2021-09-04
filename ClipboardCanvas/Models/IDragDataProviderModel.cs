using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace ClipboardCanvas.Models
{
    public interface IDragDataProviderModel
    {
        Task SetDragData(DataPackage data);
    }
}
