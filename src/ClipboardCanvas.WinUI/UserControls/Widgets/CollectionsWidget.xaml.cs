using ClipboardCanvas.Sdk.ViewModels.Widgets;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Windows.System;

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

        private async void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (args.KeyboardAccelerator.Modifiers != VirtualKeyModifiers.Control)
                return;

            var keyCode = (int)args.KeyboardAccelerator.Key;
            if (keyCode < 49 || keyCode > 57)
                return;

            var index = keyCode - 49;
            if (ItemsSource?.ElementAtOrDefault(index) is CollectionViewModel collection)
                await collection.OpenCollectionCommand.ExecuteAsync(null);
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

        private async void CollectionItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is not Border control)
                return;

            control.Background = (Brush)Resources["ButtonBackgroundPointerOver"];
            control.BorderBrush = (Brush)Resources["ButtonBorderBrushPointerOver"];

            if (control.DataContext is CollectionViewModel viewModel)
                await viewModel.OpenCollectionCommand.ExecuteAsync(null);
        }

        public IList<CollectionViewModel>? ItemsSource
        {
            get => (IList<CollectionViewModel>?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IList<CollectionViewModel>), typeof(CollectionsWidget), new PropertyMetadata(null));

        public ICommand? AddCollectionCommand
        {
            get => (ICommand?)GetValue(AddCollectionCommandProperty);
            set => SetValue(AddCollectionCommandProperty, value);
        }
        public static readonly DependencyProperty AddCollectionCommandProperty =
            DependencyProperty.Register(nameof(AddCollectionCommand), typeof(ICommand), typeof(CollectionsWidget), new PropertyMetadata(null));
    }
}
