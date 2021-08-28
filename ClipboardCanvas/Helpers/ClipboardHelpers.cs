using Windows.ApplicationModel.DataTransfer;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.Helpers
{
    public static class ClipboardHelpers
    {
        private static readonly object clipboardLock = new object();

        public static void CopyDataPackage(DataPackage dataPackage)
        {
            if (dataPackage != null)
            {
                lock (clipboardLock)
                {
                    dataPackage.RequestedOperation = DataPackageOperation.Copy;
                    Clipboard.SetContent(dataPackage);
                    Clipboard.Flush();
                }
            }
        }

        public static SafeWrapper<DataPackageView> GetClipboardData()
        {
            SafeWrapper<DataPackageView> dataPackage = SafeWrapperRoutines.SafeWrap(Clipboard.GetContent);
            
            return dataPackage;
        }
    }
}
