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
        #region Public Properties

        public IStorageItem Item { get; private set; }

        public BasePastedContentTypeDataModel ContentType { get; set; }

        #endregion

        #region Constructor

        public CollectionItemViewModel(IStorageItem item)
            : this(item, null)
        {
        }

        public CollectionItemViewModel(IStorageItem item, BasePastedContentTypeDataModel contentType)
        {
            this.Item = item;
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

            StorageFile file = Item as StorageFile;
            if (file != null && ReferenceFile.IsReferenceFile(file))
            {
                ReferenceFile referenceFile = await ReferenceFile.GetFile(file);

                if (referenceFile.ReferencedItem == null)
                {
                    return;
                }

                if (referenceFile.ReferencedItem is StorageFile referencedFile)
                {
                    await Launcher.LaunchFileAsync(referencedFile);
                }
                else
                {
                    await Launcher.LaunchFolderAsync(referenceFile.ReferencedItem as StorageFolder);
                }
            }
            else
            {
                if (file != null)
                {
                    await Launcher.LaunchFileAsync(file);
                }
                else
                {
                    await Launcher.LaunchFolderAsync(Item as StorageFolder);
                }
            }
        }

        public async Task OpenContainingFolder()
        {
            await OpenContainingFolder(true);
        }

        public async Task OpenContainingFolder(bool checkForReference)
        {
            IStorageFolder folder;
            IStorageItem itemToSelect;

            if (checkForReference && Item is StorageFile file && ReferenceFile.IsReferenceFile(file))
            {
                ReferenceFile referenceFile = await ReferenceFile.GetFile(file);

                if (referenceFile.ReferencedItem == null)
                {
                    folder = await StorageHelpers.ToStorageItem<StorageFolder>(Path.GetDirectoryName(Item.Path));
                    itemToSelect = file;
                }
                else
                {
                    folder = await StorageHelpers.ToStorageItem<StorageFolder>(Path.GetDirectoryName(referenceFile.ReferencedItem.Path));
                    itemToSelect = referenceFile.ReferencedItem;
                }
            }
            else
            {
                folder = await StorageHelpers.ToStorageItem<StorageFolder>(Path.GetDirectoryName(Item.Path));
                itemToSelect = Item;
            }

            if (folder != null)
            {
                FolderLauncherOptions launcherOptions = new FolderLauncherOptions();

                if (Item != null)
                {
                    launcherOptions.ItemsToSelect.Add(itemToSelect);
                }

                await Launcher.LaunchFolderAsync(folder, launcherOptions);
            }
        }

        public void DangerousUpdateFile(IStorageItem item)
        {
            this.Item = item;
        }

        #endregion
    }
}
