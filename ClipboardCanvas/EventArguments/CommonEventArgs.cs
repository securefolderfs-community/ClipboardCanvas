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

        public readonly bool tipWithDelay;

        public CollectionItemsInitializationStartedEventArgs(CollectionsContainerViewModel containerViewModel, string infoText, bool tipWithDelay = false)
        {
            this.containerViewModel = containerViewModel;
            this.infoText = infoText;
            this.tipWithDelay = tipWithDelay;
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

    public class CollectionErrorRaisedEventArgs
    {
        public readonly SafeWrapperResult result;

        public CollectionErrorRaisedEventArgs(SafeWrapperResult result)
        {
            this.result = result;
        }
    }
}
