using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml;
using Microsoft.UI;

namespace ClipboardCanvas.Helpers
{
    public static class ThemeHelper
    {
        public static UISettings UISettings { get; private set; }

        public static ApplicationTheme CurrentTheme => Application.Current.RequestedTheme;

        public static void Initialize()
        {
            // TODO: Regression
            //MainWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            //MainWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

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
            // TODO: Regression
            switch (CurrentTheme)
            {
                case ApplicationTheme.Light:
                    {
                        //MainWindow.TitleBar.ButtonForegroundColor = Colors.Black;
                        break;
                    }

                case ApplicationTheme.Dark:
                    {
                        //MainWindow.TitleBar.ButtonForegroundColor = Colors.White;
                        break;
                    }
            }
        }
    }
}
