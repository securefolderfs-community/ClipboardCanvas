using Windows.UI.Xaml.Media.Animation;
using ClipboardCanvas.Enums;

namespace ClipboardCanvas.Helpers
{
    public static class AnimationHelpers
    {
        public static NavigationTransitionInfo ToNavigationTransition(this NavigationTransitionType transitionType)
        {
            switch (transitionType)
            {
                default:
                case NavigationTransitionType.Suppress:
                    return new SuppressNavigationTransitionInfo();

                case NavigationTransitionType.DrillInTransition:
                    return new DrillInNavigationTransitionInfo();

                case NavigationTransitionType.EntranceTransition:
                    return new EntranceNavigationTransitionInfo();
            }
        }
    }
}
