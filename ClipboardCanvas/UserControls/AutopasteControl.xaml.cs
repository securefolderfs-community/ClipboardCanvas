using Windows.UI.Xaml.Controls;

using ClipboardCanvas.ViewModels.UserControls.Autopaste;
using Windows.UI.Xaml;
using ClipboardCanvas.Services;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class AutopasteControl : UserControl
    {
        private IAutopasteService AutopasteService { get; } = Ioc.Default.GetService<IAutopasteService>();

        public AutopasteControlViewModel ViewModel
        {
            get => (AutopasteControlViewModel)GetValue(ViewModelProperty);
            set
            {
                SetValue(ViewModelProperty, value);
                if (this.AutopasteService is Services.Implementation.AutopasteService autopasteService)
                {
                    autopasteService.AutopasteControlViewModel = value;
                }
            }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
          "ViewModel",
          typeof(AutopasteControlViewModel),
          typeof(AutopasteControl),
          null
        );

        public AutopasteControl()
        {
            this.InitializeComponent();
        }
    }
}
