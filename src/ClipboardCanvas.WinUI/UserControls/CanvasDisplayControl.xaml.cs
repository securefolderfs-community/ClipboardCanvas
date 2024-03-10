using ClipboardCanvas.Sdk.ViewModels.Controls.Canvases;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.UserControls
{
    public sealed partial class CanvasDisplayControl : UserControl
    {
        public CanvasDisplayControl()
        {
            InitializeComponent();
        }

        public BaseCanvasViewModel? CanvasViewModel
        {
            get => (BaseCanvasViewModel?)GetValue(CanvasViewModelProperty);
            set => SetValue(CanvasViewModelProperty, value);
        }
        public static readonly DependencyProperty CanvasViewModelProperty =
            DependencyProperty.Register(nameof(CanvasViewModel), typeof(BaseCanvasViewModel), typeof(CanvasDisplayControl), new PropertyMetadata(null));
    }
}
