using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

using ClipboardCanvas.ViewModels.UserControls.Collections;

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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem != null)
            {
                CollectionsItemsHolder.ScrollIntoView(ViewModel.SelectedItem);
            }
        }

        private void RootGrid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            e.Handled = true;
            ((sender as FrameworkElement).DataContext as BaseCollectionViewModel).OpenCollectionCommand.Execute(null);
        }

        private void RootGrid_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            (sender as Grid).Background = (Brush)Resources["ButtonBackgroundPointerOver"];
            (sender as Grid).BorderBrush = (Brush)Resources["ButtonBorderBrushPointerOver"];
        }

        private void RootGrid_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            (sender as Grid).Background = (Brush)Resources["ButtonBackgroundPressed"];
            (sender as Grid).BorderBrush = (Brush)Resources["ButtonBorderBrushPressed"];
        }

        private void RootGrid_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            (sender as Grid).Background = (Brush)Resources["ButtonBackground"];
            (sender as Grid).BorderBrush = (Brush)Resources["ButtonBorderBrush"];
        }
    }
}
