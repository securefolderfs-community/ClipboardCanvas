using ClipboardCanvas.DataModels;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.Pages;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClipboardCanvas.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePageViewModel ViewModel
        {
            get => (HomePageViewModel)DataContext;
            set => DataContext = value;
        }

        public HomePage()
        {
            this.InitializeComponent();

            this.ViewModel = new HomePageViewModel();
        }
    }
}
