using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using ClipboardCanvas.ViewModels.UserControls;

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

        private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            this.ViewModel?.DefaultKeyboardAcceleratorInvokedCommand?.Execute(args);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.ViewModel?.ItemClickCommand?.Execute(e);
        }
    }
}
