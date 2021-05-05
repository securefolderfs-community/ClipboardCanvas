using ClipboardCanvas.ViewModels.UserControls;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class SuggestedActionsOptionsControl : UserControl
    {
        public SuggestedActionsControlViewModel ViewModel
        {
            get => (SuggestedActionsControlViewModel)DataContext;
            set => DataContext = value;
        }

        public SuggestedActionsOptionsControl()
        {
            this.InitializeComponent();

            this.ViewModel = new SuggestedActionsControlViewModel();
        }
    }
}
