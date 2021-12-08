using Microsoft.UI.Xaml.Controls;

using ClipboardCanvas.ViewModels.UserControls.SimpleCanvasDisplay;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.SimpleCanvasDisplay
{
    public sealed partial class UrlSimpleCanvasControl : UserControl
    {
        public UrlSimpleCanvasViewModel ViewModel
        {
            get => (UrlSimpleCanvasViewModel)DataContext;
        }

        public UrlSimpleCanvasControl()
        {
            this.InitializeComponent();
        }
    }
}
