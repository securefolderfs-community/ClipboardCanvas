using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.CanvasDisplay
{
    public sealed partial class MediaCanvasControl : UserControl
    {
        public MediaCanvasViewModel ViewModel
        {
            get => (MediaCanvasViewModel)DataContext;
        }

        public MediaCanvasControl()
        {
            this.InitializeComponent();
        }
    }
}
