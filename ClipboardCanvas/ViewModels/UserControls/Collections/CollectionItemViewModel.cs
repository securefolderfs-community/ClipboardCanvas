using System;
using System.IO;
using Windows.System;
using Windows.Storage;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

using ClipboardCanvas.Models;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Contexts;
using ClipboardCanvas.ViewModels.UserControls.InAppNotifications;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Services;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class CollectionItemViewModel : CanvasItem, ICollectionItemModel
    {
        private IDialogService DialogService { get; } = Ioc.Default.GetService<IDialogService>();

        public IStorageItem Item { get; private set; }

        public BaseContentTypeModel ContentType { get; set; }

        public OperationContext OperationContext { get; set; }

        public CollectionItemViewModel(IStorageItem item)
            : this(item, null)
        {
        }

        public CollectionItemViewModel(IStorageItem item, BaseContentTypeModel contentType)
            : base(item)
        {
            this.Item = item;
            this.sourceItem = null;
            this.ContentType = contentType;
            this.OperationContext = new OperationContext();
        }

        public async Task OpenFile()
        {
            if (Item == null)
            {
                return;
            }

            string sourceItemPath = (await SourceItem).Path;
            if (sourceItemPath.EndsWith(".exe"))
            {
                IInAppNotification notification = DialogService.GetNotification();
                notification.ViewModel.NotificationText = "UWP cannot open executable (.exe) files";
                notification.ViewModel.ShownButtons = InAppNotificationButtonType.OkButton;

                await notification.ShowAsync(4000);
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
    }
}
