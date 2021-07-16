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
            set
            {
                DataContext = value;
                SimpleCanvasPreviewModelAccessor?.PropertyValueUpdated(ViewModel);
            }
        }

        public static readonly DependencyProperty CollectionModelProperty =
            DependencyProperty.Register(nameof(CollectionModel), typeof(ICollectionModel), typeof(SimpleCanvasPreviewControl), new PropertyMetadata(null));
        public ICollectionModel CollectionModel
        {
            get { return (ICollectionModel)GetValue(CollectionModelProperty); }
            set { SetValue(CollectionModelProperty, value); }
        }


        public IControlPropertyAccessorModel<IReadOnlyCanvasPreviewModel> SimpleCanvasPreviewModelAccessor
        {
            get { return (IControlPropertyAccessorModel<IReadOnlyCanvasPreviewModel>)GetValue(SimpleCanvasPreviewModelAccessorProperty); }
            set { SetValue(SimpleCanvasPreviewModelAccessorProperty, value); }
        }

        public static readonly DependencyProperty SimpleCanvasPreviewModelAccessorProperty =
            DependencyProperty.Register(nameof(SimpleCanvasPreviewModelAccessor), typeof(IControlPropertyAccessorModel<IReadOnlyCanvasPreviewModel>), typeof(SimpleCanvasPreviewControl), new PropertyMetadata(null));


        public SimpleCanvasPreviewControl()
        {
            this.InitializeComponent();

            this.ViewModel = new SimpleCanvasPreviewControlViewModel(this);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                // ViewModel is not null meaning the value changed and therefore
                // call SimpleCanvasPreviewModelAccessor.PropertyValueUpdated(), because it could not be called before the control was loaded
                SimpleCanvasPreviewModelAccessor?.PropertyValueUpdated(ViewModel);
            }
        }
    }
}
