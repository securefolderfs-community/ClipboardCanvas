using ClipboardCanvas.Shared.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.UserControls.Canvases
{
    public sealed partial class ImageCanvasControl : UserControl
    {
        public ImageCanvasControl()
        {
            InitializeComponent();
        }

        public bool IsImmersed
        {
            get => (bool)GetValue(IsImmersedProperty);
            set => SetValue(IsImmersedProperty, value);
        }
        public static readonly DependencyProperty IsImmersedProperty =
            DependencyProperty.Register(nameof(IsImmersed), typeof(bool), typeof(ImageCanvasControl), new PropertyMetadata(false));

        public IImage? Image
        {
            get => (IImage?)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(nameof(Image), typeof(IImage), typeof(ImageCanvasControl), new PropertyMetadata(null));
    }
}
