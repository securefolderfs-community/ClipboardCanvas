using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

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

        private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            this.ViewModel?.DefaultKeyboardAcceleratorInvokedCommand?.Execute(args);
        }
    }
}
