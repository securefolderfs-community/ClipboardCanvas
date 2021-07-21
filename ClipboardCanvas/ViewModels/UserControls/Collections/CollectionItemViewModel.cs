using System;
using System.IO;
using Windows.System;
using Windows.Storage;
using System.Threading.Tasks;

using ClipboardCanvas.Models;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Contexts;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class CollectionItemViewModel : CanvasFile, ICollectionItemModel
    {
        #region Public Properties

        public IStorageItem Item { get; private set; }

        public BasePastedContentTypeDataModel ContentType { get; set; }

        public OperationContext OperationContext { get; set; }

        #endregion

        #region Constructor

        public CollectionItemViewModel(IStorageItem item)
            : this(item, null)
        {
        }

        public CollectionItemViewModel(IStorageItem item, BasePastedContentTypeDataModel contentType)
            : base(item)
        {
            this.Item = item;
            this.sourceItem = null;
            this.ContentType = contentType;
            this.OperationContext = new OperationContext();
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

        #endregion
    }
}
