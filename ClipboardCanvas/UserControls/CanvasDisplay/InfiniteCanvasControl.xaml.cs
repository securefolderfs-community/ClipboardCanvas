using Windows.UI.Xaml.Controls;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.CanvasDisplay
{
    public sealed partial class InfiniteCanvasControl : UserControl
    {
        public InfiniteCanvasViewModel ViewModel
        {
            get => (InfiniteCanvasViewModel)DataContext;
        }

        public InfiniteCanvasControl()
        {
            this.InitializeComponent();
        }
    }
}
