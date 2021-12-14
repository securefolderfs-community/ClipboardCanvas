using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;

using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.Contexts.Operations;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.CanavsPasteModels
{
    public class FallbackPasteModel : BasePasteModel
    {
        private bool _isFolder;

        public StorageItemThumbnail ItemThumbnail { get; private set; }

        public string FileName { get; private set; }

        public string FilePath { get; private set; }

        public DateTime DateCreated { get; private set; }

        public DateTime DateModified { get; private set; }

        public BitmapImage FileIcon { get; private set; }

        public FallbackPasteModel(ICanvasItemReceiverModel canvasFileReceiver, IOperationContextReceiver operationContextReceiver)
            : base(canvasFileReceiver, operationContextReceiver)
        {
        }

        protected override Task<SafeWrapper<CanvasItem>> GetCanvasFileFromExtension(ICanvasItemReceiverModel canvasFileReceiver)
        {
            return Task.FromResult<SafeWrapper<CanvasItem>>((null, SafeWrapperResult.SUCCESS));
        }

        protected override Task<SafeWrapperResult> SaveDataToFile()
        {
            return Task.FromResult(SafeWrapperResult.SUCCESS);
        }

        protected override Task<SafeWrapperResult> SetDataFromDataPackage(DataPackageView dataPackage)
        {
            return Task.FromResult(SafeWrapperResult.SUCCESS);
        }

        public override async Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
        {
            // Read file properties
            if (item is StorageFile file)
            {
                ItemThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem);
            }
            else if (item is StorageFolder folder)
            {
                _isFolder = true;
                ItemThumbnail = await folder.GetThumbnailAsync(ThumbnailMode.SingleItem);
            }

            this.FileName = item.Name;
            this.FilePath = item.Path;
            this.DateCreated = item.DateCreated.DateTime;

            var properties = await item.GetBasicPropertiesAsync();
            this.DateModified = properties.DateModified.DateTime;

            FileIcon = new BitmapImage();
            await FileIcon.SetSourceAsync(ItemThumbnail);

            return SafeWrapperResult.SUCCESS;
        }

        public override bool CheckCanPasteReference()
        {
            return !_isFolder;
        }

        public void UpdatePathProperty(string newPath)
        {
            this.FilePath = newPath;
            this.FileName = Path.GetFileName(newPath);
        }

        #region IDisposable

        public override void Dispose()
        {
            base.Dispose();

            ItemThumbnail?.Dispose();

            ItemThumbnail = null;
            FileIcon = null;
        }

        #endregion
    }
}
