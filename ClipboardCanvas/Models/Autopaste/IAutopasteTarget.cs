using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using System.ComponentModel;

using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.Models.Autopaste
{
    public interface IAutopasteTarget : INotifyPropertyChanged
    {
        string DisplayName { get; }

        string TargetPath { get; }

        Task<SafeWrapperResult> PasteData(DataPackageView dataPackage, CancellationToken cancellationToken);
    }
}
