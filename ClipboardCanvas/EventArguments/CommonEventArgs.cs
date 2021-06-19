using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public readonly CollectionsContainerViewModel containerViewModel;

        public readonly string infoText;

        public readonly TimeSpan tipShowDelay;

        public CollectionItemsInitializationStartedEventArgs(CollectionsContainerViewModel containerViewModel, string infoText)
            : this(containerViewModel, infoText, TimeSpan.Zero)
        {
        }

        public CollectionItemsInitializationStartedEventArgs(CollectionsContainerViewModel containerViewModel, string infoText, TimeSpan tipShowDelay)
        {
            this.containerViewModel = containerViewModel;
            this.infoText = infoText;
            this.tipShowDelay = tipShowDelay;
        }
    }

    public class CollectionItemsInitializationFinishedEventArgs : EventArgs
    {
        public readonly CollectionsContainerViewModel containerViewModel;

        public CollectionItemsInitializationFinishedEventArgs(CollectionsContainerViewModel containerViewModel)
        {
            this.containerViewModel = containerViewModel;
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
