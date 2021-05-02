using ClipboardCanvas.ViewModels.UserControls;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class WindowTitleBarControl : UserControl
    {
        public WindowTitleBarControlViewModel ViewModel
        {
            get => (WindowTitleBarControlViewModel)DataContext;
            set => DataContext = value;
        }

        public WindowTitleBarControl()
        {
            this.InitializeComponent();

            this.ViewModel = new WindowTitleBarControlViewModel();
        }
    }
}
