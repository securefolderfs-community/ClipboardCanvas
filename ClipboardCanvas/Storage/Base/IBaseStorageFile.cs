using System.IO;
using System.Threading.Tasks;

namespace ClipboardCanvas.Storage.Base
{
    public interface IBaseStorageFile : IBaseStorageItem
    {
        Task<Stream> OpenAsync(FileAccess access);
    }
}
