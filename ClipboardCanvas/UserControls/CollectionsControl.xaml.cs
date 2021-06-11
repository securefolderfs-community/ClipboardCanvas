using ClipboardCanvas.ViewModels.UserControls;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class CollectionsControl : UserControl
    {
        public CollectionsControlViewModel ViewModel
        {
            get => (CollectionsControlViewModel)DataContext;
            set => DataContext = value;
        }

        public CollectionsControl()
        {
            this.InitializeComponent();

            this.ViewModel = new CollectionsControlViewModel();
        }
    }
}
