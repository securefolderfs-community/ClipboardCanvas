using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.UserControls.Widgets
{
    public sealed partial class CollectionsWidget : UserControl
    {
        public CollectionsWidget()
        {
            InitializeComponent();
        }

        private void CollectionItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Border control)
                return;

            control.Background = (Brush)Resources["ButtonBackgroundPointerOver"];
            control.BorderBrush = (Brush)Resources["ButtonBorderBrushPointerOver"];
        }

        private void CollectionItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Border control)
                return;

            control.Background = (Brush)Resources["ButtonBackground"];
            control.BorderBrush = (Brush)Resources["ButtonBorderBrush"];
        }

        private void CollectionItem_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Border control)
                return;

            control.Background = (Brush)Resources["ButtonBackgroundPressed"];
            control.BorderBrush = (Brush)Resources["ButtonBorderBrushPressed"];
        }

        private void CollectionItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is not Border control)
                return;

            control.Background = (Brush)Resources["ButtonBackgroundPointerOver"];
            control.BorderBrush = (Brush)Resources["ButtonBorderBrushPointerOver"];
        }

        public IList? ItemsSource
        {
            get => (IList?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IList), typeof(CollectionsWidget), new PropertyMetadata(null));
    }
}
