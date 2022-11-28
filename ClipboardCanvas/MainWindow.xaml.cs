using ClipboardCanvas.Helpers;
using ClipboardCanvas.Pages;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static MainWindow Instance;

        public IntPtr Hwnd { get; private set; }

        public AppWindow? AppWindow { get; private set; }

        public MainWindowContentPage MainWindowContentPage => MainPageHost;

        public MainWindow()
        {
            Instance = this;
            this.InitializeComponent();

            EnsureSafeInitialization();
        }

        private void EnsureSafeInitialization()
        {
            try
            {
                // Get AppWindow
                Hwnd = WindowNative.GetWindowHandle(this);
                var mainWindowWndId = Win32Interop.GetWindowIdFromWindow(Hwnd);
                AppWindow = AppWindow.GetFromWindowId(mainWindowWndId);

                // Set title
                if (AppWindow is not null)
                {
                    AppWindow.Title = "Clipboard Canvas";
                }
                else
                {
                    Title = "Clipboard Canvas";
                }

                if (AppWindowTitleBar.IsCustomizationSupported())
                {
                    // Extend title bar
                    AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

                    // Set window buttons background to transparent
                    AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
                    AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                }
                else
                {
                    this.ExtendsContentIntoTitleBar = true;
                    SetTitleBar(MainWindowContentPage.WindowTitleBar.CustomTitleBar);
                }

                ThemeHelper.Initialize();
            }
            catch (Exception ex)
            {
                LoggingHelpers.SafeLogExceptionToFile(ex);
            }
        }
    }
}
