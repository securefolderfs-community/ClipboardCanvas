using ClipboardCanvas.Helpers;
using Windows.ApplicationModel.Core;
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
        public static ApplicationViewTitleBar TitleBar { get; private set; }

        public static CoreApplicationViewTitleBar CoreTitleBar { get; private set; }

        public MainPage()
        {
            this.InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            TitleBar = ApplicationView.GetForCurrentView().TitleBar;
            CoreTitleBar = CoreApplication.GetCurrentView().TitleBar;

            CoreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(WindowTitleBar.DraggableRegion);

            ThemeHelper.Initialize();

            CoreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            WindowTitleBar.CompactOverlay.Margin = new Thickness(0, 0, sender.SystemOverlayRightInset, 0);
        }
    }
}
