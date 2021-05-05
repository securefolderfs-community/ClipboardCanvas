using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class NavigationToolBarControl : UserControl, INavigationToolBarControlView
    {
        public NavigationToolBarControlViewModel ViewModel
        {
            get => (NavigationToolBarControlViewModel)DataContext;
            set => DataContext = value;
        }

        public INavigationControlModel NavigationControlModel => NavigationControls?.ViewModel;

        public ISuggestedActionsControlModel SuggestedActionsControlModel => SuggestedActions?.ViewModel;

        public NavigationToolBarControl()
        {
            this.InitializeComponent();

            this.ViewModel = new NavigationToolBarControlViewModel(this);
        }
    }
}
