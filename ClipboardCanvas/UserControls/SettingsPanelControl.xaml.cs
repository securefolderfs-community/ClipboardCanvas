using Windows.UI.Xaml.Controls;
using ClipboardCanvas.ViewModels.UserControls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class SettingsPanelControl : UserControl
    {
        public SettingsPanelControlViewModel ViewModel
        {
            get => (SettingsPanelControlViewModel)DataContext;
            set => DataContext = value;
        }

        public SettingsPanelControl()
        {
            this.InitializeComponent();

            this.ViewModel = new SettingsPanelControlViewModel();
        }
    }
}
