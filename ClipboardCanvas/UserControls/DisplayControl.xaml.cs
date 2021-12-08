using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Diagnostics;

using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.Pages;
using ClipboardCanvas.ViewModels.UserControls;
using ClipboardCanvas.Services;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class DisplayControl : UserControl, IDisplayControlView
    {
        private INavigationService NavigationService { get; } = Ioc.Default.GetService<INavigationService>();

        public DisplayControlViewModel ViewModel
        {
            get => (DisplayControlViewModel)DataContext;
            set => DataContext = value;
        }

        public bool IntroductionPanelLoad
        {
            get => MainWindow.Instance.MainWindowContentPage.IntroductionPanelLoad;
            set => MainWindow.Instance.MainWindowContentPage.IntroductionPanelLoad = value;
        }

        public IWindowTitleBarControlModel WindowTitleBarControlModel { get; set; }

        public INavigationToolBarControlModel NavigationToolBarControlModel { get; set; }

        public IIntroductionScreenPanelModel IntroductionScreenPanelModel { get; set; }

        public IPasteCanvasPageModel PasteCanvasPageModel
        {
            get
            {
                if ((DisplayFrame.Content as Page)?.DataContext is CanvasPageViewModel viewModel)
                {
                    return viewModel;
                }

                return null;
            }
        }

        public ICollectionPreviewPageModel CollectionPreviewPageModel
        {
            get
            {
                if ((DisplayFrame.Content as Page)?.DataContext is CollectionPreviewPageViewModel viewModel)
                {
                    return viewModel;
                }

                return null;
            }
        }

        public DisplayControl()
        {
            this.InitializeComponent();
            
            this.ViewModel = new DisplayControlViewModel(this);
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Configure navigation
            if (this.NavigationService is Services.Implementation.NavigationService navigationServiceImpl)
            {
                navigationServiceImpl.DisplayFrame = this.DisplayFrame;
                navigationServiceImpl.CheckCollectionAvailabilityBeforePageNavigation = this.ViewModel.CheckCollectionAvailabilityBeforePageNavigation;
            }
            else
            {
                Debugger.Break(); // Shouldn't even happen
            }

            // Initialize the rest when the view is loaded
            await this.ViewModel.InitializeAfterLoad();
        }
    }
}
