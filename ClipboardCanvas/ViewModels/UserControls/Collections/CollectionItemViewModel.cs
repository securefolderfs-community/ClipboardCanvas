using System;
using System.IO;
using Windows.System;
using Windows.Storage;
using System.Threading.Tasks;

using ClipboardCanvas.Models;
using ClipboardCanvas.ReferenceItems;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.DataModels.PastedContentDataModels;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class CollectionItemViewModel : ICollectionItemModel
    {
        #region Private Members

        private IStorageItem _sourceItem;

        #endregion

        #region Public Properties

        public IStorageItem Item { get; private set; }

        public Task<IStorageItem> SourceItem => GetSourceItem();

        public BasePastedContentTypeDataModel ContentType { get; set; }

        #endregion

        #region Constructor

        public CollectionItemViewModel(IStorageItem item)
            : this(item, null)
        {
        }

        public CollectionItemViewModel(IStorageItem item, BasePastedContentTypeDataModel contentType)
        {
            DangerousUpdateFile(item);
            this.ContentType = contentType;
        }

        #endregion

        #region ICollectionsContainerItemModel

        public async Task OpenFile()
        {
            if (Item == null)
            {
                return;
            }

            if (await SourceItem is StorageFile file)
            {
                await Launcher.LaunchFileAsync(file);
            }
            else if (await SourceItem is StorageFolder folder)
            {
                await Launcher.LaunchFolderAsync(folder);
            }
        }

        public async Task OpenContainingFolder()
        {
            await OpenContainingFolder(true);
        }

        public async Task OpenContainingFolder(bool checkForReference)
        {
            IStorageFolder parentFolder;
            IStorageItem itemToSelect;

            if (checkForReference)
            {
                string parentPath = Path.GetDirectoryName((await SourceItem).Path);
                parentFolder = await StorageHelpers.ToStorageItem<StorageFolder>(parentPath);
                itemToSelect = await SourceItem;
            }
            else
            {
                string parentPath = Path.GetDirectoryName(Item.Path);
                parentFolder = await StorageHelpers.ToStorageItem<StorageFolder>(parentPath);
                itemToSelect = Item;
            }

            if (parentFolder != null)
            {
                FolderLauncherOptions launcherOptions = new FolderLauncherOptions();

                if (itemToSelect != null)
                {
                    launcherOptions.ItemsToSelect.Add(itemToSelect);
                }

                await Launcher.LaunchFolderAsync(parentFolder, launcherOptions);
            }
        }

        public void DangerousUpdateFile(IStorageItem item)
        {
            this.Item = item;
            this._sourceItem = null;
        }

        #endregion

        #region Private Helpers

        private async Task<IStorageItem> GetSourceItem()
        {
            if (_sourceItem != null)
            {
                return _sourceItem;
            }

            if (Item is StorageFile file && ReferenceFile.IsReferenceFile(file))
            {
                ReferenceFile referenceFile = await ReferenceFile.GetFile(file);
                _sourceItem = referenceFile.ReferencedItem;
            }
            else
            {
                _sourceItem = Item;
            }

            return _sourceItem;
        }

        #endregion
    }
}
