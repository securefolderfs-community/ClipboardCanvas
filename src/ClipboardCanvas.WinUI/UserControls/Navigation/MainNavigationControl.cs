using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;

namespace ClipboardCanvas.WinUI.UserControls.Navigation
{
    /// <inheritdoc cref="FrameNavigationControl"/>
    internal sealed partial class MainNavigationControl : FrameNavigationControl
    {
        /// <inheritdoc/>
        public override Dictionary<Type, Type> TypeBinding { get; } = new();

        /// <inheritdoc/>
        protected override bool NavigateFrame(Type pageType, object parameter, NavigationTransitionInfo? transitionInfo)
        {
            return ContentFrame.Navigate(pageType, parameter, transitionInfo);
        }
    }
}
