using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.CanvasDisplay
{
    public sealed partial class TextCanvasControl : UserControl
    {
        public TextCanvasViewModel ViewModel
        {
            get => (TextCanvasViewModel)DataContext;
        }

        public TextCanvasControl()
        {
            this.InitializeComponent();
        }
    }
}
