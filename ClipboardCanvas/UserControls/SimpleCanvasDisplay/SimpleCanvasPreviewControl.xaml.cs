using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls.CanvasPreview;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.SimpleCanvasDisplay
{
    public sealed partial class SimpleCanvasPreviewControl : UserControl, IBaseCanvasPreviewControlView
    {
        public SimpleCanvasPreviewControlViewModel ViewModel
        {
            get => (SimpleCanvasPreviewControlViewModel)DataContext;
            set => DataContext = value;
        }

        public static readonly DependencyProperty CollectionModelProperty =
            DependencyProperty.Register(nameof(CollectionModel), typeof(ICollectionModel), typeof(SimpleCanvasPreviewControl), new PropertyMetadata(null));
        public ICollectionModel CollectionModel
        {
            get { return (ICollectionModel)GetValue(CollectionModelProperty); }
            set { SetValue(CollectionModelProperty, value); }
        }


        public static readonly DependencyProperty SimpleCanvasPreviewModelProperty =
            DependencyProperty.Register(nameof(SimpleCanvasPreviewModel), typeof(IReadOnlyCanvasPreviewModel), typeof(SimpleCanvasPreviewControl), new PropertyMetadata(null));
        public IReadOnlyCanvasPreviewModel SimpleCanvasPreviewModel
        {
            get { return (IReadOnlyCanvasPreviewModel)GetValue(SimpleCanvasPreviewModelProperty); }
            set { SetValue(SimpleCanvasPreviewModelProperty, value); }
        }

        public SimpleCanvasPreviewControl()
        {
            this.InitializeComponent();

            this.ViewModel = new SimpleCanvasPreviewControlViewModel(this);
            this.SimpleCanvasPreviewModel = ViewModel;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
