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

        public MainWindowContentPage MainWindowContentPage => MainPageHost;

        public static ApplicationViewTitleBar TitleBar { get; private set; }

        public static CoreApplicationViewTitleBar CoreTitleBar { get; private set; }

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
                Title = "Clipboard Canvas";
                ExtendsContentIntoTitleBar = true;
                SetTitleBar(MainWindowContentPage.WindowTitleBar.DraggableRegion);

                ThemeHelper.Initialize();
            }
            catch (Exception ex)
            {
                LoggingHelpers.SafeLogExceptionToFile(ex);
            }
        }
    }
}
