using System;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.UserControls;

namespace ClipboardCanvas.EventArguments
{
    public class OpenNewCanvasRequestedEventArgs : EventArgs
    {
        public OpenNewCanvasRequestedEventArgs()
        {
        }
    }

    public class GoToHomePageRequestedEventArgs : EventArgs
    {
        public GoToHomePageRequestedEventArgs()
        {
        }
    }

    public class CollectionItemsInitializationStartedEventArgs : EventArgs
    {
        public readonly CollectionViewModel collectionViewModel;

        public readonly string infoText;

        public readonly TimeSpan tipShowDelay;

        public CollectionItemsInitializationStartedEventArgs(CollectionViewModel containerViewModel, string infoText)
            : this(containerViewModel, infoText, TimeSpan.Zero)
        {
        }

        public CollectionItemsInitializationStartedEventArgs(CollectionViewModel collectionViewModel, string infoText, TimeSpan tipShowDelay)
        {
            this.collectionViewModel = collectionViewModel;
            this.infoText = infoText;
            this.tipShowDelay = tipShowDelay;
        }
    }

    public class CollectionItemsInitializationFinishedEventArgs : EventArgs
    {
        public readonly CollectionViewModel collectionViewModel;

        public CollectionItemsInitializationFinishedEventArgs(CollectionViewModel collectionViewModel)
        {
            this.collectionViewModel = collectionViewModel;
        }
    }

    public class CollectionErrorRaisedEventArgs : EventArgs
    {
        public readonly SafeWrapperResult result;

        public CollectionErrorRaisedEventArgs(SafeWrapperResult result)
        {
            this.result = result;
        }
    }

    public class CheckCanvasPageNavigationRequestedEventArgs : EventArgs
    {
        public readonly bool alsoRefreshActions;

        public readonly SafeWrapperResult error;

        public CheckCanvasPageNavigationRequestedEventArgs(bool alsoRefreshActions = false, SafeWrapperResult error = null)
        {
            this.alsoRefreshActions = alsoRefreshActions;
            this.error = error;
        }
    }
}
