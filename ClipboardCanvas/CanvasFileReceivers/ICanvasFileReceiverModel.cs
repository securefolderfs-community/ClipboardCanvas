using System.Threading.Tasks;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.CanvasFileReceivers
{
    public interface ICanvasFileReceiverModel
    {
        Task<SafeWrapper<CanvasItem>> CreateNewCanvasFolder(string folderName = null);

        Task<SafeWrapper<CanvasItem>> CreateNewCanvasFileFromExtension(string extension);

        Task<SafeWrapper<CanvasItem>> CreateNewCanvasFile(string fileName);
    }
}
