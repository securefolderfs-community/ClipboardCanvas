using ClipboardCanvas.Sdk.ViewModels.Views.Settings;
using ClipboardCanvas.WinUI.Views.Settings;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;

namespace ClipboardCanvas.WinUI.UserControls.Navigation
{
    /// <inheritdoc cref="FrameNavigationControl"/>
    internal sealed partial class SettingsNavigationControl : FrameNavigationControl
    {
        /// <inheritdoc/>
        public override Dictionary<Type, Type> TypeBinding { get; } = new()
        {
            { typeof(GeneralSettingsViewModel), typeof(GeneralSettingsPage) },
            //{ typeof(PreferencesSettingsViewModel), typeof(PreferencesSettingsPage) },
            { typeof(AboutSettingsViewModel), typeof(AboutSettingsPage) }
        };

        /// <inheritdoc/>
        protected override bool NavigateFrame(Type pageType, object parameter, NavigationTransitionInfo? transitionInfo)
        {
            transitionInfo ??= new EntranceNavigationTransitionInfo();
            return ContentFrame.Navigate(pageType, parameter, transitionInfo);
        }
    }
}
