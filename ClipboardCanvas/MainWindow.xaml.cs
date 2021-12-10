using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;

using ClipboardCanvas.Helpers;
using ClipboardCanvas.Pages;
using Vanara.PInvoke;
using System;

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

        public MainWindowContentPage MainWindowContentPage => MainWindowFrame.Content as MainWindowContentPage;

        public static ApplicationViewTitleBar TitleBar { get; private set; }

        public static CoreApplicationViewTitleBar CoreTitleBar { get; private set; }

        public MainWindow()
        {
            this.InitializeComponent();
            Instance = this;
            MainWindowFrame.Navigate(typeof(MainWindowContentPage), null, new SuppressNavigationTransitionInfo());

            Initialize();
        }

        private void Initialize()
        {
            Title = "Clipboard Canvas";
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(MainWindowContentPage.WindowTitleBar.DraggableRegion);

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            int wndLong = User32.GetWindowLong(hwnd, User32.WindowLongFlags.GWL_STYLE);
            User32.SetWindowLong(hwnd, User32.WindowLongFlags.GWL_STYLE, wndLong | (int)User32.WindowStyles.WS_SYSMENU);

            ThemeHelper.Initialize();
        }
    }
}
