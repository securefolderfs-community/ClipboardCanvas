using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.CanvasDisplay
{
    public sealed partial class FallbackCanvasControl : UserControl
    {
        public FallbackCanvasViewModel ViewModel
        {
            get => (FallbackCanvasViewModel)DataContext;
        }

        public FallbackCanvasControl()
        {
            this.InitializeComponent();
        }

        private async void Image_DragStarting(Windows.UI.Xaml.UIElement sender, Windows.UI.Xaml.DragStartingEventArgs args)
        {
            IReadOnlyList<IStorageItem> dragData = await ViewModel.ProvideDragData();

            args.Data.SetData(StandardDataFormats.StorageItems, dragData);
        }
    }
}
