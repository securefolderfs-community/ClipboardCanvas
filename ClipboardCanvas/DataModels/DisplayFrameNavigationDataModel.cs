using System;
using Windows.UI.Xaml.Media.Animation;

using ClipboardCanvas.Enums;
using ClipboardCanvas.Models;

namespace ClipboardCanvas.DataModels
{
    public class DisplayFrameNavigationDataModel : IEquatable<DisplayFrameNavigationDataModel>
    {
        public readonly DisplayPageType pageType;

        public readonly NavigationTransitionInfo transitionInfo;

        public readonly ICollectionsContainerModel collectionContainer;

        public bool simulateNavigation;

        public DisplayFrameNavigationDataModel(DisplayPageType pageType, ICollectionsContainerModel collectionsControlModel, bool simulateNavigation = false)
            : this(pageType, collectionsControlModel, new DrillInNavigationTransitionInfo(), simulateNavigation)
        {
        }

        public DisplayFrameNavigationDataModel(DisplayPageType pageType, ICollectionsContainerModel collectionsControlModel, NavigationTransitionInfo transitionInfo, bool simulateNavigation)
        {
            this.pageType = pageType;
            this.collectionContainer = collectionsControlModel;
            this.transitionInfo = transitionInfo;
            this.simulateNavigation = simulateNavigation;
        }

        public bool Equals(DisplayFrameNavigationDataModel other)
        {
            if (
                this.pageType == other.pageType
                && this.transitionInfo == other.transitionInfo
                && this.collectionContainer == other.collectionContainer
                && this.simulateNavigation == other.simulateNavigation
                )
            {
                return true;
            }

            return false;
        }
    }
}
