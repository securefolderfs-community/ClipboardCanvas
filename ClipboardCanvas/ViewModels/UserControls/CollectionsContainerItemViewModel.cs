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
    public class CollectionsContainerItemViewModel : ICollectionsContainerItemModel
    {
        #region Public Properties

        public StorageFile File { get; private set; }

        public BasePastedContentTypeDataModel ContentType { get; set; }

        #endregion

        #region Constructor

        public CollectionsContainerItemViewModel(StorageFile file)
            : this(file, null)
        {
        }

        public CollectionsContainerItemViewModel(StorageFile file, BasePastedContentTypeDataModel contentType)
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

                if (referenceFile.ReferencedFile == null)
                {
                    return;
                }

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

        public void DangerousUpdateFile(StorageFile file)
        {
            this.File = file;
        }

        #endregion
    }
}
