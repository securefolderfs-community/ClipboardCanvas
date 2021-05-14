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

    public class CollectionItemsInitializationStartedEventArgs : EventArgs
    {
        public readonly CollectionsContainerViewModel containerViewModel;

        public CollectionItemsInitializationStartedEventArgs(CollectionsContainerViewModel containerViewModel)
        {
            this.containerViewModel = containerViewModel;
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
}
