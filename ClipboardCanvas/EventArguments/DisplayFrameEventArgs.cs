using System;

using ClipboardCanvas.Enums;
using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.UserControls.Collections;

namespace ClipboardCanvas.DisplayFrameEventArgs
{
    public class NavigationStartedEventArgs : BaseDisplayFrameNavigationEventArgs
    {
        public NavigationStartedEventArgs(ICollectionModel collection, CanvasType canvasType, DisplayPageType pageType)
            : base(collection, canvasType, pageType)
        {
        }
    }

    public class NavigationFinishedEventArgs : BaseDisplayFrameNavigationEventArgs
    {
        public readonly bool success;

        public readonly CollectionItemViewModel collectionItemToLoad;

        public NavigationFinishedEventArgs(bool success, CollectionItemViewModel collectionItemToLoad, ICollectionModel collection, CanvasType canvasType, DisplayPageType pageType)
            : base(collection, canvasType, pageType)
        {
            this.success = success;
            this.collectionItemToLoad = collectionItemToLoad;
        }
    }

    public abstract class BaseDisplayFrameNavigationEventArgs : EventArgs
    {
        public readonly ICollectionModel collection;

        public readonly CanvasType canvasType;

        public readonly DisplayPageType pageType;

        public BaseDisplayFrameNavigationEventArgs(ICollectionModel collection, CanvasType canvasType, DisplayPageType pageType)
        {
            this.collection = collection;
            this.canvasType = canvasType;
            this.pageType = pageType;
        }
    }
}
