using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.Pages;
using ClipboardCanvas.ViewModels.UserControls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class DisplayControl : UserControl, IDisplayControlView
    {
        public DisplayControlViewModel ViewModel
        {
            get => (DisplayControlViewModel)DataContext;
            set => DataContext = value;
        }

        public IWindowTitleBarControlModel WindowTitleBarControlModel { get; set; }

        public INavigationToolBarControlModel NavigationToolBarControlModel { get; set; }

        public IPasteCanvasModel PasteCanvasModel
        {
            get
            {
                if ((DisplayFrame.Content as Page)?.DataContext is CanvasPageViewModel viewModel)
                {
                    return viewModel.PasteCanvasModel;
                }

                return null;
            }
        }

        public DisplayControl()
        {
            this.InitializeComponent();

            this.ViewModel = new DisplayControlViewModel(this);
        }

        // TODO: Move the event handler to the ViewModel
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // We can't initialize ViewModel.NavigationToolBarControlModel in the constructor, because it didn't load in yet
            this.ViewModel.InitializeAfterLoad();
        }
    }
}
