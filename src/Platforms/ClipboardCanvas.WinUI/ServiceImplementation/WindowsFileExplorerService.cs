using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.WinUI.Storage;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
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
        public async Task OpenAppFolderAsync(CancellationToken cancellationToken = default)
        {
            await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder).AsTask(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task OpenInFileExplorerAsync(IFolder folder, IStorableChild? highlight, CancellationToken cancellationToken = default)
        {
            var options = new FolderLauncherOptions();
            if (highlight is not null)
            {
                var windowsStorable = (IStorageItem?)(highlight as WindowsStorageFile)?.storage ?? (highlight as WindowsStorageFolder)?.storage;
                windowsStorable ??= highlight is IFile
                        ? await StorageFile.GetFileFromPathAsync(highlight.Id)
                        : await StorageFolder.GetFolderFromPathAsync(highlight.Id);
                
                options.ItemsToSelect.Add(windowsStorable);
            }

            await Launcher.LaunchFolderPathAsync(folder.Id, options).AsTask(cancellationToken);
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
