using Microsoft.UI.Xaml.Controls;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.ModelViews;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.CanvasDisplay
{
    public sealed partial class MarkdownCanvasControl : UserControl
    {
        public MarkdownCanvasViewModel ViewModel
        {
            get => (MarkdownCanvasViewModel)DataContext;
        }

        public MarkdownCanvasControl()
        {
            this.InitializeComponent();

            ContentText.ContextFlyout = null;
        }
    }
}
