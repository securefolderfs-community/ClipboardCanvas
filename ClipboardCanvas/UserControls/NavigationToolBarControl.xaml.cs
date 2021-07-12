using ClipboardCanvas.ViewModels.UserControls;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class NavigationToolBarControl : UserControl
    {
        public NavigationToolBarControlViewModel ViewModel
        {
            get => (NavigationToolBarControlViewModel)DataContext;
            set => DataContext = value;
        }

        public NavigationToolBarControl()
        {
            this.InitializeComponent();

            this.ViewModel = new NavigationToolBarControlViewModel();
        }
    }
}
