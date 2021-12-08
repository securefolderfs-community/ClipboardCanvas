using System.Threading.Tasks;
using Windows.Storage;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.CanvasFileReceivers
{
    public interface ICanvasItemReceiverModel
    {
        Task<SafeWrapper<CanvasItem>> CreateNewCanvasFolder(string folderName = null);

        Task<SafeWrapper<CanvasItem>> CreateNewCanvasItemFromExtension(string extension);

        Task<SafeWrapper<CanvasItem>> CreateNewCanvasItem(string fileName);

        Task<SafeWrapperResult> DeleteItem(IStorageItem itemToDelete, bool permanently);
    }
}
