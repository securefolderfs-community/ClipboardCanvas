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

                if (referenceFile.ReferencedFile == null)
                {
                    return;
                }

                await Launcher.LaunchFileAsync(referenceFile.ReferencedFile);
            }
            else
            {
                await Launcher.LaunchFileAsync(File);
            }
        }

        public async Task OpenContainingFolder()
        {
            await OpenContainingFolder(true);
        }

        public async Task OpenContainingFolder(bool checkForReference)
        {
            IStorageFolder folder;
            IStorageFile fileToSelect;

            if (checkForReference && ReferenceFile.IsReferenceFile(File))
            {
                ReferenceFile referenceFile = await ReferenceFile.GetFile(File);

                if (referenceFile.ReferencedFile == null)
                {
                    folder = await StorageHelpers.ToStorageItem<StorageFolder>(Path.GetDirectoryName(File.Path));
                    fileToSelect = File;
                }
                else
                {
                    folder = await StorageHelpers.ToStorageItem<StorageFolder>(Path.GetDirectoryName(referenceFile.ReferencedFile.Path));
                    fileToSelect = referenceFile.ReferencedFile;
                }
            }
            else
            {
                folder = await StorageHelpers.ToStorageItem<StorageFolder>(Path.GetDirectoryName(File.Path));
                fileToSelect = File;
            }

            if (folder != null)
            {
                FolderLauncherOptions launcherOptions = new FolderLauncherOptions();

                if (File != null)
                {
                    launcherOptions.ItemsToSelect.Add(fileToSelect);
                }

                await Launcher.LaunchFolderAsync(folder, launcherOptions);
            }
        }

        public void DangerousUpdateFile(StorageFile file)
        {
            this.File = file;
        }

        #endregion
    }
}
