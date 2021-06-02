using System;
using Windows.UI.Xaml.Media.Animation;
using ClipboardCanvas.Enums;

namespace ClipboardCanvas.DataModels.Navigation
{
    public class SettingsFrameNavigationDataModel : IEquatable<SettingsFrameNavigationDataModel>
    {
        public readonly SettingsPageType pageType;

        public readonly NavigationTransitionInfo transitionInfo;
        
        public SettingsFrameNavigationDataModel(SettingsPageType pageType)
            : this(pageType, new EntranceNavigationTransitionInfo())
        {
        }

        public SettingsFrameNavigationDataModel(SettingsPageType pageType, NavigationTransitionInfo transitionInfo)
        {
            this.pageType = pageType;
            this.transitionInfo = transitionInfo;
        }

        public bool Equals(SettingsFrameNavigationDataModel other)
        {
            if (
                this.pageType == other.pageType
                )
            {
                return true;
            }

            return false;
        }
    }
}
