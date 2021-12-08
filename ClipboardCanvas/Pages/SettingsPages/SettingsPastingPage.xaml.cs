using Microsoft.UI.Xaml.Controls;
using ClipboardCanvas.ViewModels.Pages.SettingsPages;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClipboardCanvas.Pages.SettingsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPastingPage : Page
    {
        public SettingsPastingPageViewModel ViewModel
        {
            get => (SettingsPastingPageViewModel)DataContext;
            set => DataContext = value;
        }

        public SettingsPastingPage()
        {
            this.InitializeComponent();

            this.ViewModel = new SettingsPastingPageViewModel();
        }
    }
}
