using ClipboardCanvas.Sdk.Services;
using OwlCore.Storage;
using OwlCore.Storage.SystemIO;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;

namespace ClipboardCanvas.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IFileExplorerService"/>
    internal sealed class WindowsFileExplorerService : IFileExplorerService
    {
        /// <inheritdoc/>
        public Task OpenAppFolderAsync(CancellationToken cancellationToken = default)
        {
            return Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder).AsTask(cancellationToken);
        }

        /// <inheritdoc/>
        public Task OpenInFileExplorerAsync(IFolder folder, CancellationToken cancellationToken = default)
        {
            return Launcher.LaunchFolderPathAsync(folder.Id).AsTask(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IFolder?> PickFolderAsync(CancellationToken cancellationToken = default)
        {
            var folderPicker = new FolderPicker();
            InitializeObject(folderPicker);

            folderPicker.FileTypeFilter.Add("*");
            var folder = await folderPicker.PickSingleFolderAsync().AsTask(cancellationToken);
            if (folder is null)
                return null;

            // Since we're running natively on Windows we can avoid using Windows.Storage because it's very slow
            return new SystemFolder(folder.Path);
            //return new Storage.WindowsStorageFolder(folder);
        }

        private static void InitializeObject(object obj)
        {
            _ = obj;
#if WINDOWS
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Instance);
            WinRT.Interop.InitializeWithWindow.Initialize(obj, hwnd);
#endif
        }
    }
}
