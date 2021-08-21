using Windows.UI.Xaml.Controls;

using ClipboardCanvas.ViewModels.Pages.SettingsPages;
using ClipboardCanvas.ModelViews;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ClipboardCanvas.Pages.SettingsPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsAboutPage : Page, ISettingsAboutPageView
    {
        public SettingsAboutPageViewModel ViewModel
        {
            get => (SettingsAboutPageViewModel)DataContext;
            set => DataContext = value;
        }

        public SettingsAboutPage()
        {
            this.InitializeComponent();

            this.ViewModel = new SettingsAboutPageViewModel(this);
        }

        public bool IntroductionPanelLoad
        {
            get => MainPage.Instance.IntroductionPanelLoad;
            set => MainPage.Instance.IntroductionPanelLoad = value;
        }
    }
}
