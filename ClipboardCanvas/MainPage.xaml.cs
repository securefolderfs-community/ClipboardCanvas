using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ClipboardCanvas
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            Window.Current.SetTitleBar(WindowTitleBar.DraggableRegion);
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            var coreTitleBar = sender as CoreApplicationViewTitleBar;
            WindowTitleBar.CompactOverlay.Margin = new Thickness(0, 0, coreTitleBar.SystemOverlayRightInset, 0);
        }
    }
}
