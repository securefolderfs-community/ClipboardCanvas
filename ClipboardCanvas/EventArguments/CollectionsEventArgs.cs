using System;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.UserControls.Collections;

namespace ClipboardCanvas.EventArguments.Collections
{
    public abstract class BaseCollectionEventArgs : EventArgs
    {
        public readonly BaseCollectionViewModel baseCollectionViewModel;

        public BaseCollectionEventArgs(BaseCollectionViewModel baseCollectionViewModel)
        {
            this.baseCollectionViewModel = baseCollectionViewModel;
        }
    }

    public class CollectionRemovedEventArgs : BaseCollectionEventArgs
    {
        public CollectionRemovedEventArgs(BaseCollectionViewModel baseCollectionViewModel)
            : base(baseCollectionViewModel)
        {
        }
    }

    public class CollectionAddedEventArgs : BaseCollectionEventArgs
    {
        public CollectionAddedEventArgs(BaseCollectionViewModel baseCollectionViewModel)
            : base(baseCollectionViewModel)
        {
        }
    }

    public class CollectionSelectionChangedEventArgs : BaseCollectionEventArgs
    {
        public CollectionSelectionChangedEventArgs(BaseCollectionViewModel baseCollectionViewModel)
            : base(baseCollectionViewModel)
        {
        }
    }

    public class CollectionOpenRequestedEventArgs : BaseCollectionEventArgs
    {
        public CollectionOpenRequestedEventArgs(BaseCollectionViewModel baseCollectionViewModel)
            : base(baseCollectionViewModel)
        {
        }
    }

    public class RemoveCollectionRequestedEventArgs : BaseCollectionEventArgs
    {
        public RemoveCollectionRequestedEventArgs(BaseCollectionViewModel baseCollectionViewModel)
            : base(baseCollectionViewModel)
        {
        }
    }

    public class CheckRenameCollectionRequestedEventArgs : BaseCollectionEventArgs
    {
        public readonly string newName;

        public bool canRename;

        public CheckRenameCollectionRequestedEventArgs(BaseCollectionViewModel baseCollectionViewModel, string newName)
            : base(baseCollectionViewModel)
        {
            this.newName = newName;
        }
    }

    public class CollectionItemsInitializationStartedEventArgs : BaseCollectionEventArgs
    {
        public CollectionItemsInitializationStartedEventArgs(BaseCollectionViewModel baseCollectionViewModel)
            : base(baseCollectionViewModel)
        {
        }
    }

    public class CollectionItemsInitializationFinishedEventArgs : BaseCollectionEventArgs
    {
        public CollectionItemsInitializationFinishedEventArgs(BaseCollectionViewModel baseCollectionViewModel)
            : base(baseCollectionViewModel)
        {
        }
    }

    public class CollectionErrorRaisedEventArgs : EventArgs
    {
        public readonly SafeWrapperResult safeWrapperResult;

        public CollectionErrorRaisedEventArgs(SafeWrapperResult safeWrapperResult)
        {
            this.safeWrapperResult = safeWrapperResult;
        }
    }

    public class CanvasLoadFailedEventArgs : EventArgs
    {
        public readonly SafeWrapperResult error;

        public CanvasLoadFailedEventArgs(SafeWrapperResult error = null)
        {
            this.error = error;
        }
    }

    public class GoToHomepageRequestedEventArgs : EventArgs
    {
        public GoToHomepageRequestedEventArgs()
        {
        }
    }
}
