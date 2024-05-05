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
            IsEditing = false;
        }

        public IImage? Image
        {
            get => (IImage?)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(nameof(Image), typeof(IImage), typeof(ImageCanvasControl), new PropertyMetadata(null));

        public bool IsEditing
        {
            get => (bool)GetValue(IsEditingProperty);
            set => SetValue(IsEditingProperty, value);
        }
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(ImageCanvasControl), new PropertyMetadata(false));

        public bool WasAltered
        {
            get => (bool)GetValue(WasAlteredProperty);
            set => SetValue(WasAlteredProperty, value);
        }
        public static readonly DependencyProperty WasAlteredProperty =
            DependencyProperty.Register(nameof(WasAltered), typeof(bool), typeof(ImageCanvasControl), new PropertyMetadata(false));
    }
}
