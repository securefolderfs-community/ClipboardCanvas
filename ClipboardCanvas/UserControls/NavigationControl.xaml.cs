using ClipboardCanvas.ViewModels.UserControls;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

            // TODO: Use AttachedViewModel and set there DataContext?
        }
    }
}
