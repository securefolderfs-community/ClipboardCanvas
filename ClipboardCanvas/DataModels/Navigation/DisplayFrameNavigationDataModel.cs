using Windows.UI.Xaml.Media.Animation;

using ClipboardCanvas.Enums;

namespace ClipboardCanvas.DataModels.Navigation
{
    public class DisplayFrameNavigationDataModel
    {
        public readonly DisplayPageType pageType;

        public readonly NavigationTransitionInfo transitionInfo;

        public readonly DisplayFrameParameterDataModel parameter;

        public bool simulateNavigation;

        public DisplayFrameNavigationDataModel(DisplayPageType pageType, DisplayFrameParameterDataModel parameter, bool simulateNavigation = false)
            : this(pageType, parameter, new DrillInNavigationTransitionInfo(), simulateNavigation)
        {
        }

        public DisplayFrameNavigationDataModel(DisplayPageType pageType, DisplayFrameParameterDataModel parameter, NavigationTransitionInfo transitionInfo, bool simulateNavigation)
        {
            this.pageType = pageType;
            this.parameter = parameter;
            this.transitionInfo = transitionInfo;
            this.simulateNavigation = simulateNavigation;
        }
    }
}
