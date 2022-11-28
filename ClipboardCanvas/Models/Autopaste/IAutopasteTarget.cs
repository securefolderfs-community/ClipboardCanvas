using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using System.ComponentModel;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.DataModels;

namespace ClipboardCanvas.Models.Autopaste
{
    public interface IAutopasteTarget : INotifyPropertyChanged
    {
        string DisplayName { get; }

        string TargetPath { get; }

        Task<SafeWrapper<CanvasItem>> PasteData(DataPackageView dataPackage, CancellationToken cancellationToken);
    }
}
