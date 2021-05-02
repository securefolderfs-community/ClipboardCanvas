using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.CanvasDisplay
{
    public sealed partial class ImageCanvasControl : UserControl
    {
        public ImageCanvasViewModel ViewModel
        {
            get => (ImageCanvasViewModel)DataContext;
        }

        public ImageCanvasControl()
        {
            this.InitializeComponent();
        }
    }
}
