using System;
using System.Threading.Tasks;
using Windows.Storage;

using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Enums;

namespace ClipboardCanvas.DataModels
{
    public class InfiniteCanvasItem : CanvasItem
    {
        public StorageFile ConfigurationFile { get; private set; }

        public StorageFile CanvasPreviewImageFile { get; private set; }

        public InfiniteCanvasItem(IStorageItem associatedItem)
            : base(associatedItem)
        {
        }

        public InfiniteCanvasItem(IStorageItem associatedItem, IStorageItem sourceItem)
            : base(associatedItem, sourceItem)
        {
        }

        public async Task<SafeWrapperResult> InitializeCanvasFolder()
        {
            if ((await SourceItem) is not StorageFolder infiniteCanvasFolder)
            {
                return new SafeWrapperResult(OperationErrorCode.AccessUnauthorized, new UnauthorizedAccessException(), "Couldn't initialize Infinite Canvas folder.");
            }

            string configurationFileName = Constants.FileSystem.INFINITE_CANVAS_CONFIGURATION_FILENAME;
            string canvasPreviewImageFileName = Constants.FileSystem.INFINITE_CANVAS_PREVIEW_IMAGE_FILENAME;

            SafeWrapper<StorageFile> result = await FilesystemOperations.CreateFile(infiniteCanvasFolder, configurationFileName, CreationCollisionOption.OpenIfExists);
            ConfigurationFile = result;

            if (!result)
            {
                return result;
            }

            result = await FilesystemOperations.CreateFile(infiniteCanvasFolder, canvasPreviewImageFileName, CreationCollisionOption.OpenIfExists);
            CanvasPreviewImageFile = result.Result;

            return result;
        }
    }
}
