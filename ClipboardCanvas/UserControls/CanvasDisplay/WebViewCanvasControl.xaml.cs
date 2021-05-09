using Windows.UI.Xaml.Controls;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.CanvasDisplay
{
    public sealed partial class WebViewCanvasControl : UserControl
    {
        public WebViewCanvasViewModel ViewModel
        {
            get => (WebViewCanvasViewModel)DataContext;
        }

        public WebViewCanvasControl()
        {
            this.InitializeComponent();
        }
    }
}
