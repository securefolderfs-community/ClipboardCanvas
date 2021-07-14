using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using ClipboardCanvas.ViewModels;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class SearchControl : UserControl
    {
        public SearchControlViewModel ViewModel
        {
            get => (SearchControlViewModel)DataContext;
            set => DataContext = value;
        }

        public SearchControl()
        {
            this.InitializeComponent();

            this.ViewModel = new SearchControlViewModel();
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            InputBox.Focus(FocusState.Programmatic);
        }
    }
}
