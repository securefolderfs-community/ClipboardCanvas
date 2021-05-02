using ClipboardCanvas.ViewModels.UserControls;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class AdaptiveOptionsControl : UserControl
    {
        public AdaptiveOptionsControlViewModel ViewModel
        {
            get => (AdaptiveOptionsControlViewModel)DataContext;
            set => DataContext = value;
        }

        public AdaptiveOptionsControl()
        {
            this.InitializeComponent();

            this.ViewModel = new AdaptiveOptionsControlViewModel();
        }
    }
}
