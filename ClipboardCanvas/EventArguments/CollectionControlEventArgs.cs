using System;
using System.Collections.Generic;
using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.UserControls;

namespace ClipboardCanvas.EventArguments.CollectionControl
{
    public class CollectionRemovedEventArgs : EventArgs
    {
        public readonly ICollectionsContainerModel removedCollection;

        public CollectionRemovedEventArgs(ICollectionsContainerModel removedCollection)
        {
            this.removedCollection = removedCollection;
        }
    }

    public class CollectionAddedEventArgs : EventArgs
    {
        public readonly ICollectionsContainerModel addedCollection;

        public CollectionAddedEventArgs(ICollectionsContainerModel addedCollection)
        {
            this.addedCollection = addedCollection;
        }
    }

    public class CollectionSelectionChangedEventArgs : EventArgs
    {
        public readonly ICollectionsContainerModel selectedCollection;

        public CollectionSelectionChangedEventArgs(ICollectionsContainerModel selectedCollection)
        {
            this.selectedCollection = selectedCollection;
        }
    }

    public class CollectionOpenRequestedEventArgs : EventArgs
    {
        public readonly CollectionsContainerViewModel openedCollection;

        public CollectionOpenRequestedEventArgs(CollectionsContainerViewModel openedCollection)
        {
            this.openedCollection = openedCollection;
        }
    }

    public class CollectionItemsRefreshRequestedEventArgs : EventArgs
    {
        public readonly ICollectionsContainerModel selectedCollection;

        public CollectionItemsRefreshRequestedEventArgs(ICollectionsContainerModel selectedCollection)
        {
            this.selectedCollection = selectedCollection;
        }
    }
}
