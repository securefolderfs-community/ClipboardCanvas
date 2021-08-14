using System.Threading.Tasks;
using Windows.Storage;

using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.CanvasLoadModels
{
    public interface ILoadModel
    {
        Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item);
    }
}
