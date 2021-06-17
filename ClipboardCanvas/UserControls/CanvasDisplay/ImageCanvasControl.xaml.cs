using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
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

        // TODO: Move to ViewModel
        private void Image_DragStarting(Windows.UI.Xaml.UIElement sender, Windows.UI.Xaml.DragStartingEventArgs args)
        {
            IReadOnlyList<IStorageItem> dragData = ViewModel.ProvideDragData();

            args.Data.SetStorageItems(dragData);
        }
    }
}
