using ClipboardCanvas.Sdk.Enums;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.Shared.Extensions;
using ClipboardCanvas.UI.ServiceImplementation;
using ClipboardCanvas.WinUI.UserControls.Navigation;
using Microsoft.UI.Xaml.Media.Animation;
using System.Linq;
using System.Threading.Tasks;

namespace ClipboardCanvas.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="INavigationService"/>
    public sealed class WindowsNavigationService : BaseNavigationService
    {
        /// <inheritdoc/>
        protected override async Task<bool> BeginNavigationAsync(IViewDesignation? view, NavigationType navigationType)
        {
            if (NavigationControl is null)
                return false;

            switch (navigationType)
            {
                case NavigationType.Backward:
                    {
                        if (NavigationControl is not FrameNavigationControl frameNavigation)
                            return false;

                        if (!frameNavigation.ContentFrame.CanGoBack)
                            return false;

                        // Navigate back
                        frameNavigation.ContentFrame.GoBack();

                        var contentType = frameNavigation.Content?.GetType();
                        if (contentType is null)
                            return false;

                        var targetType = frameNavigation.TypeBinding.GetByKeyOrValue(contentType);
                        var backTarget = Views.FirstOrDefault(x => x.GetType() == targetType);
                        if (backTarget is not null)
                            CurrentView = backTarget;

                        return true;
                    }

                case NavigationType.Forward:
                    {
                        if (NavigationControl is not FrameNavigationControl frameNavigation)
                            return false;

                        if (!frameNavigation.ContentFrame.CanGoForward)
                            return false;

                        // Navigate forward
                        frameNavigation.ContentFrame.GoForward();

                        var contentType = frameNavigation.ContentFrame.Content?.GetType();
                        if (contentType is null)
                            return false;

                        var targetType = frameNavigation.TypeBinding.GetByKeyOrValue(contentType);
                        var forwardTarget = Views.FirstOrDefault(x => x.GetType() == targetType);
                        if (forwardTarget is not null)
                            CurrentView = forwardTarget;
                        return true;
                    }

                default:
                case NavigationType.Chained:
                    {
                        if (view is null)
                            return false;

                        return await NavigationControl.NavigateAsync(view, (NavigationTransitionInfo?)null);
                    }
            }
        }
    }
}
