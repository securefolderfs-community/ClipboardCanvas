using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.CanvasFileReceivers
{
    public class InfiniteCanvasFileReceiver : ICanvasItemReceiverModel
    {
        private CanvasItem _infiniteCanvasFolder;

        public InfiniteCanvasFileReceiver(CanvasItem infiniteCanvasFolder)
        {
            this._infiniteCanvasFolder = infiniteCanvasFolder;
        }

        public Task<SafeWrapper<CanvasItem>> CreateNewCanvasFolder(string folderName = null)
        {
            // TODO: In the future, allow cascading Infinite Canvases
            return Task.FromResult(new SafeWrapper<CanvasItem>(null, new SafeWrapperResult(OperationErrorCode.InvalidOperation, new InvalidOperationException(), "Cannot create folders inside Infinite Canvas.")));
        }

        public async Task<SafeWrapper<CanvasItem>> CreateNewCanvasItemFromExtension(string extension)
        {
            string fileName = DateTime.Now.ToString(Constants.FileSystem.CANVAS_FILE_FILENAME_DATE_FORMAT);
            fileName = $"{fileName}{extension}";

            return await CreateNewCanvasItem(fileName);
        }

        public async Task<SafeWrapper<CanvasItem>> CreateNewCanvasItem(string fileName)
        {
            if (_infiniteCanvasFolder == null || await _infiniteCanvasFolder.SourceItem is not StorageFolder folder)
            {
                return new SafeWrapper<CanvasItem>(null, OperationErrorCode.NotFound, new DirectoryNotFoundException(), "The Infinite Canvas folder was not found.");
            }

            SafeWrapper<StorageFile> file = await FilesystemOperations.CreateFile(folder, fileName);

            return (new CanvasItem(file.Result), file.Details);
        }

        public async Task<SafeWrapperResult> DeleteItem(IStorageItem itemToDelete, bool permanently)
        {
            return await FilesystemOperations.DeleteItem(itemToDelete, permanently);
        }
    }
}
