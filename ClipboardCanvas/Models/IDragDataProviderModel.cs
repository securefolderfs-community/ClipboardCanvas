using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.Models
{
    public interface IDragDataProviderModel
    {
        Task<IReadOnlyList<IStorageItem>> GetDragData();
    }
}
