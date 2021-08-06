using System;
using System.Threading.Tasks;

using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.Models
{
    public interface IInteractableCanvasControlItemModel : IDragDataProviderModel, IDisposable
    {
        Task<SafeWrapperResult> LoadContent();
    }
}
