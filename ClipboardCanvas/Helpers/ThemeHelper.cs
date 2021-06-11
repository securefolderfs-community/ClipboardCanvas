using System;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace ClipboardCanvas.Helpers
{
    public static class ThemeHelper
    {
        public static UISettings UISettings { get; private set; }

        public static ApplicationTheme CurrentTheme => Application.Current.RequestedTheme;

        public static void Initialize()
        {
            MainPage.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            MainPage.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            UISettings = new UISettings();
            UISettings.ColorValuesChanged += UISettings_ColorValuesChanged;

            RefreshTheme();
        }

        private static void UISettings_ColorValuesChanged(UISettings sender, object args)
        {
            // RefreshTheme();
        }

        public static void RefreshTheme()
        {
            switch (CurrentTheme)
            {
                case ApplicationTheme.Light:
                    {
                        MainPage.TitleBar.ButtonForegroundColor = Colors.Black;
                        break;
                    }

                case ApplicationTheme.Dark:
                    {
                        MainPage.TitleBar.ButtonForegroundColor = Colors.White;
                        break;
                    }
            }
        }
    }
}
