using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

using ClipboardCanvas.ViewModels.UserControls.Collections;
using Microsoft.UI.Xaml.Input;

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

        private void RootGrid_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            e.Handled = true;
            ((sender as FrameworkElement).DataContext as BaseCollectionViewModel).OpenCollectionCommand.Execute(null);
        }

        private void RootGrid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            (sender as Grid).Background = (Brush)Resources["ButtonBackgroundPointerOver"];
            (sender as Grid).BorderBrush = (Brush)Resources["ButtonBorderBrushPointerOver"];
        }

        private void RootGrid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            (sender as Grid).Background = (Brush)Resources["ButtonBackgroundPressed"];
            (sender as Grid).BorderBrush = (Brush)Resources["ButtonBorderBrushPressed"];
        }

        private void RootGrid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            (sender as Grid).Background = (Brush)Resources["ButtonBackground"];
            (sender as Grid).BorderBrush = (Brush)Resources["ButtonBorderBrush"];
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            this.ViewModel?.DragOverCommand?.Execute(e);
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            this.ViewModel?.DropCommand?.Execute(e);
        }

        private void EditBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            ((sender as FrameworkElement)?.DataContext as BaseCollectionViewModel)?.RenameBoxKeyDownCommand?.Execute(e);
        }

        private void EditBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ((sender as FrameworkElement)?.DataContext as BaseCollectionViewModel)?.RenameBoxLostFocusCommand?.Execute(e);
        }
    }
}
