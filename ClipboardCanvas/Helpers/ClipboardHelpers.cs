using ClipboardCanvas.DataModels;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace ClipboardCanvas.Helpers
{
    public static class ClipboardHelpers
    {
        public static async Task<SafeWrapper<DataPackageView>> GetClipboardData()
        {
            SafeWrapper<DataPackageView> dataPackage = await SafeWrapperRoutines.SafeWrapAsync(() => Task.FromResult(Clipboard.GetContent()), StaticExceptionReporters.ClipboardGetDataExceptionReporter);

            return dataPackage;
        }
    }
}
