using Microsoft.UI.Xaml.Controls;

using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls.CanvasPreview;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.CanvasDisplay
{
    public sealed partial class CanvasPreviewControl : UserControl, IBaseCanvasPreviewControlView
    {
        public CanvasPreviewControlViewModel ViewModel
        {
            get => (CanvasPreviewControlViewModel)DataContext;
            set => DataContext = value;
        }

        public ICollectionModel CollectionModel { get; set; }

        public CanvasPreviewControl()
        {
            this.InitializeComponent();

            this.ViewModel = new CanvasPreviewControlViewModel(this);
        }
    }
}
