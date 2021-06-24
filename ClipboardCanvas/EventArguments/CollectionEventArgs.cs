using System;
using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.UserControls;

namespace ClipboardCanvas.EventArguments.Collections
{
    public class CollectionRemovedEventArgs : EventArgs
    {
        public readonly ICollectionModel collectionModel;

        public CollectionRemovedEventArgs(ICollectionModel collectionModel)
        {
            this.collectionModel = collectionModel;
        }
    }

    public class CollectionAddedEventArgs : EventArgs
    {
        public readonly ICollectionModel collectionModel;

        public CollectionAddedEventArgs(ICollectionModel collectionModel)
        {
            this.collectionModel = collectionModel;
        }
    }

    public class CollectionSelectionChangedEventArgs : EventArgs
    {
        public readonly ICollectionModel collectionModel;

        public CollectionSelectionChangedEventArgs(ICollectionModel collectionModel)
        {
            this.collectionModel = collectionModel;
        }
    }

    public class CollectionOpenRequestedEventArgs : EventArgs
    {
        public readonly CollectionViewModel collectionViewModel;

        public CollectionOpenRequestedEventArgs(CollectionViewModel collectionViewModel)
        {
            this.collectionViewModel = collectionViewModel;
        }
    }

    public class RemoveCollectionRequestedEventArgs : EventArgs
    {
        public readonly CollectionViewModel collectionViewModel;

        public RemoveCollectionRequestedEventArgs(CollectionViewModel collectionViewModel)
        {
            this.collectionViewModel = collectionViewModel;
        }
    }

    public class CheckRenameCollectionRequestedEventArgs : EventArgs
    {
        public readonly CollectionViewModel collectionViewModel;

        public readonly string newName;

        public bool canRename;

        public CheckRenameCollectionRequestedEventArgs(CollectionViewModel collectionViewModel, string newName)
        {
            this.collectionViewModel = collectionViewModel;
            this.newName = newName;
        }
    }
}
