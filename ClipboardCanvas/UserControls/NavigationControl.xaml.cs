using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using ClipboardCanvas.ViewModels.UserControls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class NavigationControl : UserControl
    {
        public NavigationControlViewModel ViewModel
        {
            get => (NavigationControlViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
          "ViewModel",
          typeof(NavigationControlViewModel),
          typeof(NavigationControl),
          null
        );

        public NavigationControl()
        {
            this.InitializeComponent();
        }

        private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            this.ViewModel?.DefaultKeyboardAcceleratorInvokedCommand?.Execute(args);
        }
    }
}
