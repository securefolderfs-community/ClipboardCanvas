using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

using ClipboardCanvas.Services;
using ClipboardCanvas.ViewModels.UserControls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class NavigationToolBarControl : UserControl
    {
        private IStatusCenterService StatusCenterService { get; } = Ioc.Default.GetService<IStatusCenterService>();

        public NavigationToolBarControlViewModel ViewModel
        {
            get => (NavigationToolBarControlViewModel)DataContext;
            set => DataContext = value;
        }

        public NavigationToolBarControl()
        {
            this.InitializeComponent();

            this.ViewModel = new NavigationToolBarControlViewModel();

            if (this.StatusCenterService is Services.Implementation.StatusCenterService statusCenterServiceImpl)
            {
                statusCenterServiceImpl.NavigationToolBarControlModel = this.ViewModel;
            }
        }
    }
}
