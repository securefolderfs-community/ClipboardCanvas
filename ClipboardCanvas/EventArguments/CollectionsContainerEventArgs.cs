using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.UserControls;
using System;

namespace ClipboardCanvas.EventArguments.CollectionsContainer
{
    public class ItemsRefreshRequestedEventArgs : EventArgs
    {
        public readonly CollectionsContainerViewModel containerViewModel;

        public ItemsRefreshRequestedEventArgs(CollectionsContainerViewModel containerViewModel)
        {
            this.containerViewModel = containerViewModel;
        }
    }

    public class RemoveCollectionRequestedEventArgs : EventArgs
    {
        public readonly CollectionsContainerViewModel containerViewModel;

        public RemoveCollectionRequestedEventArgs(CollectionsContainerViewModel containerViewModel)
        {
            this.containerViewModel = containerViewModel;
        }
    }

    public class CheckRenameCollectionRequestedEventArgs : EventArgs
    {
        public readonly CollectionsContainerViewModel containerViewModel;

        public readonly string newName;

        public bool canRename;

        public CheckRenameCollectionRequestedEventArgs(CollectionsContainerViewModel containerViewModel, string newName)
        {
            this.containerViewModel = containerViewModel;
            this.newName = newName;
        }
    }
}
