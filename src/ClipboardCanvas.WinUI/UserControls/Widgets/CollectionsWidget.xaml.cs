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

        public IList? ItemsSource
        {
            get => (IList?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IList), typeof(CollectionsWidget), new PropertyMetadata(null));

        private void RootGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Grid rootGrid)
                return;

            rootGrid.Background = (Brush)Resources["ButtonBackgroundPressed"];
            rootGrid.BorderBrush = (Brush)Resources["ButtonBorderBrushPressed"];
        }

        private void RootGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Grid rootGrid)
                return;

            rootGrid.Background = (Brush)Resources["ButtonBackgroundPointerOver"];
            rootGrid.BorderBrush = (Brush)Resources["ButtonBorderBrushPointerOver"];
        }

        private void RootGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Grid rootGrid)
                return;

            rootGrid.Background = (Brush)Resources["ButtonBackground"];
            rootGrid.BorderBrush = (Brush)Resources["ButtonBorderBrush"];
        }
    }
}
