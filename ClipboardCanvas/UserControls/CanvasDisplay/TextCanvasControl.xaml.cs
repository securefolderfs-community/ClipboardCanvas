using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.CanvasDisplay
{
    public sealed partial class TextCanvasControl : UserControl, ITextCanvasControlView
    {
        public TextCanvasViewModel ViewModel
        {
            get => (TextCanvasViewModel)DataContext;
        }

        public TextCanvasControl()
        {
            this.InitializeComponent();

            // Set the ContentText's context to null to override it in the ViewModel
            ContentText.ContextFlyout = null;
            ContentText.SelectionFlyout = null;
        }

        public bool IsTextSelected
        {
            get => ContentText.SelectedText.Length > 0;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ControlView = this;
        }

        private void ContentText_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        public void TextSelectAll()
        {
            ContentText.SelectAll();
            ContentText.Focus(FocusState.Programmatic);
        }

        public void CopySelectedText()
        {
            ContentText.CopySelectionToClipboard();
        }
    }
}
