using Microsoft.UI.Xaml.Controls;

using ClipboardCanvas.ViewModels.Pages.SettingsPages;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClipboardCanvas.Pages.SettingsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsNotificationsPage : Page
    {
        public SettingsNotificationsPageViewModel ViewModel
        {
            get => (SettingsNotificationsPageViewModel)DataContext;
            set => DataContext = value;
        }

        public SettingsNotificationsPage()
        {
            this.InitializeComponent();

            this.ViewModel = new SettingsNotificationsPageViewModel();
        }
    }
}
