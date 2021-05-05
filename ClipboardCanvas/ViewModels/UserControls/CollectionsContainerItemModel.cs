using ClipboardCanvas.DataModels;
using ClipboardCanvas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using ClipboardCanvas.Enums;
using System.IO;
using Windows.System;
using ClipboardCanvas.Helpers.FilesystemHelpers;
using ClipboardCanvas.DataModels.PastedContentDataModels;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class CollectionsContainerItemModel : ICollectionsContainerItemModel
    {
        #region Public Properties

        public StorageFile File { 
            get; 
            private set; }

        public BasePastedContentTypeDataModel ContentType { get; set; }

        #endregion

        #region Constructor

        public CollectionsContainerItemModel(StorageFile file)
            : this(file, null)
        {
        }

        public CollectionsContainerItemModel(StorageFile file, BasePastedContentTypeDataModel contentType)
        {
            this.File = file;
            this.ContentType = contentType;
        }

        #endregion

        #region ICollectionsContainerItemModel

        public async Task OpenFile()
        {
            await Launcher.LaunchFileAsync(File);
        }

        public async Task OpenContainingFolder()
        {
            StorageFolder folder = await StorageItemHelpers.ToStorageItem<StorageFolder>(Path.GetDirectoryName(File.Path));

            FolderLauncherOptions launcherOptions = new FolderLauncherOptions();
            launcherOptions.ItemsToSelect.Add(File);

            await Launcher.LaunchFolderAsync(folder, launcherOptions);
        }

        #endregion
    }
}
