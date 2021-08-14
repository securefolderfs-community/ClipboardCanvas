using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.DataModels;

namespace ClipboardCanvas.CanavsPasteModels
{
    public interface IPasteModel : IDisposable
    {
        bool IsContentAsReference { get; }

        bool CanPasteReference { get; }

        Task<SafeWrapper<CanvasItem>> PasteData(DataPackageView dataPackage, bool pasteAsReference, CancellationToken cancellationToken);
    }
}
