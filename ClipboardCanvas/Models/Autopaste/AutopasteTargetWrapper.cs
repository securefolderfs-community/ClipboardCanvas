using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace ClipboardCanvas.Models.Autopaste
{
    public sealed class AutopasteTargetWrapper : ObservableObject, IAutopasteTarget
    {
        private readonly Func<DataPackageView, CancellationToken, Task<SafeWrapper<CanvasItem>>> _pasteFunction;

        public string DisplayName { get; }

        public string TargetPath { get; }

        public AutopasteTargetWrapper(string displayName, string targetPath, Func<DataPackageView, CancellationToken, Task<SafeWrapper<CanvasItem>>> pasteFunction)
        {
            DisplayName = displayName;
            TargetPath = targetPath;
            _pasteFunction = pasteFunction;
        }

        public async Task<SafeWrapper<CanvasItem>> PasteData(DataPackageView dataPackage, CancellationToken cancellationToken)
        {
            return await _pasteFunction(dataPackage, cancellationToken);
        }
    }
}
