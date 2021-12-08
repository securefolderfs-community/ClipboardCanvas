using Microsoft.UI.Xaml.Controls;

using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;

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

        private async void Image_DragStarting(Microsoft.UI.Xaml.UIElement sender, Microsoft.UI.Xaml.DragStartingEventArgs args)
        {
            await ViewModel.SetDragData(args.Data);
        }
    }
}
