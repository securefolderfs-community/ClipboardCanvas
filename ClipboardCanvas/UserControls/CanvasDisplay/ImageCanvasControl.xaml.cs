using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.Extensions;

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

        private async void Image_DragStarting(Windows.UI.Xaml.UIElement sender, Windows.UI.Xaml.DragStartingEventArgs args)
        {
            IReadOnlyList<IStorageItem> dragData = await ViewModel.GetDragData();

            if (dragData.CheckEveryNotNull())
            {
                args.Data.SetStorageItems(dragData);
            }
        }
    }
}
