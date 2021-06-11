using Windows.UI.Xaml.Controls;

using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using Windows.UI.Xaml;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class DynamicCanvasControl : UserControl, IDynamicCanvasControlView
    {
        public DynamicCanvasControlViewModel ViewModel
        {
            get => (DynamicCanvasControlViewModel)DataContext;
            set => DataContext = value;
        }

        public ICollectionsContainerModel CollectionContainer { get; set; }

        public DynamicCanvasControl()
        {
            this.InitializeComponent();

            this.ViewModel = new DynamicCanvasControlViewModel(this);
        }
    }
}
