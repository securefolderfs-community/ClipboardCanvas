using Windows.UI.Xaml.Controls;
using ClipboardCanvas.ViewModels.UserControls.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

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
    }
}
