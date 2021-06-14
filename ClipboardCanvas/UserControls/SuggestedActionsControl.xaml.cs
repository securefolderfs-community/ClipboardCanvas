using ClipboardCanvas.ViewModels.UserControls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class SuggestedActionsControl : UserControl
    {
        public SuggestedActionsControlViewModel ViewModel
        {
            get => (SuggestedActionsControlViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(SuggestedActionsControlViewModel),
            typeof(SuggestedActionsControl),
            new PropertyMetadata(null));

        public SuggestedActionsControl()
        {
            this.InitializeComponent();

            // TODO: Use AttachedViewModel and set there DataContext?
        }
    }
}
