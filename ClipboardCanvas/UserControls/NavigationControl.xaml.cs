using ClipboardCanvas.ViewModels.UserControls;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class NavigationControl : UserControl
    {
        public NavigationControlViewModel ViewModel
        {
            get => (NavigationControlViewModel)DataContext;
            set => DataContext = value;
        }

        public NavigationControl()
        {
            this.InitializeComponent();

            this.ViewModel = new NavigationControlViewModel();
        }
    }
}
