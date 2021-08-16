using Windows.UI.Xaml.Controls;
using ClipboardCanvas.ViewModels.UserControls.Collections;
using Windows.UI.Xaml;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.Widgets
{
    public sealed partial class CollectionsWidget : UserControl
    {
        public CollectionsWidgetViewModel ViewModel
        {
            get => (CollectionsWidgetViewModel)DataContext;
            set => DataContext = value;
        }

        public CollectionsWidget()
        {
            this.InitializeComponent();

            this.ViewModel = new CollectionsWidgetViewModel();
        }

        private void RootGrid_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is BaseCollectionViewModel collectionViewModel)
            {
                this.ViewModel.OpenItem(collectionViewModel);
            }
        }
    }
}
