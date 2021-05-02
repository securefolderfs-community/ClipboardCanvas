using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.UserControls;
using System;

namespace ClipboardCanvas.EventArguments
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

    public class RenameCollectionRequestedEventArgs : EventArgs
    {
        public readonly CollectionsContainerViewModel containerViewModel;

        public readonly string newName;

        public bool renamed;

        public RenameCollectionRequestedEventArgs(CollectionsContainerViewModel containerViewModel, string newName)
        {
            this.containerViewModel = containerViewModel;
            this.newName = newName;
        }
    }
}
