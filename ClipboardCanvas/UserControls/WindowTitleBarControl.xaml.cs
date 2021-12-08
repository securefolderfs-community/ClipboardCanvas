using Microsoft.UI.Xaml.Controls;
using ClipboardCanvas.ViewModels.UserControls;
using ClipboardCanvas.Helpers;

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

        // TODO: Move to view model??
        private async void RestrictedAccess_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await InitialApplicationChecksHelpers.HandleFileSystemPermissionDialog(ViewModel);
        }
    }
}
