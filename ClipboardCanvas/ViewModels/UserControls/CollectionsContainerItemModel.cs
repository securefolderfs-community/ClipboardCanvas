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
using ClipboardCanvas.ReferenceItems;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class CollectionsContainerItemModel : ICollectionsContainerItemModel
    {
        #region Public Properties

        public StorageFile File { get; private set; }

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
            if (ReferenceFile.IsReferenceFile(File))
            {
                ReferenceFile referenceFile = await ReferenceFile.GetFile(File);
                await Launcher.LaunchFileAsync(referenceFile.ReferencedFile);
            }
            else
            {
                await Launcher.LaunchFileAsync(File);
            }
        }

        public async Task OpenContainingFolder()
        {
            IStorageFolder folder;
            IStorageFile fileToSelect;
            if (ReferenceFile.IsReferenceFile(File))
            {
                ReferenceFile referenceFile = await ReferenceFile.GetFile(File);
                folder = await StorageItemHelpers.ToStorageItem<StorageFolder>(Path.GetDirectoryName(referenceFile.ReferencedFile.Path));
                fileToSelect = referenceFile.ReferencedFile;
            }
            else
            {
                folder = await StorageItemHelpers.ToStorageItem<StorageFolder>(Path.GetDirectoryName(File.Path));
                fileToSelect = File;
            }

            FolderLauncherOptions launcherOptions = new FolderLauncherOptions();
            launcherOptions.ItemsToSelect.Add(fileToSelect);

            await Launcher.LaunchFolderAsync(folder, launcherOptions);
        }

        #endregion
    }
}
