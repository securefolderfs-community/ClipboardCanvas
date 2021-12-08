using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;

using ClipboardCanvas.ViewModels.UserControls.StatusCenter;
using ClipboardCanvas.Services;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class StatusCenterControl : UserControl
    {
        private IStatusCenterService StatusCenterService { get; } = Ioc.Default.GetService<IStatusCenterService>();

        public StatusCenterViewModel ViewModel
        {
            get => (StatusCenterViewModel)DataContext;
            set => DataContext = value;
        }

        public StatusCenterControl()
        {
            this.InitializeComponent();

            this.ViewModel = new StatusCenterViewModel();

            if (this.StatusCenterService is Services.Implementation.StatusCenterService statusCenterServiceImpl)
            {
                statusCenterServiceImpl.StatusCenterViewModel = this.ViewModel;
            }
        }
    }
}
