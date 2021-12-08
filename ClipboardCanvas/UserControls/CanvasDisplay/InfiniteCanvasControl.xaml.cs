using Microsoft.UI.Xaml.Controls;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Models;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls.CanvasDisplay
{
    public sealed partial class InfiniteCanvasControl : UserControl, IInfiniteCanvasControlView
    {
        public InfiniteCanvasViewModel ViewModel
        {
            get => (InfiniteCanvasViewModel)DataContext;
        }

        public IInteractableCanvasControlModel InteractableCanvasModel => InteractableCanvas.ViewModel;

        public InfiniteCanvasControl()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            this.ViewModel.ControlView = this;
        }
    }
}
