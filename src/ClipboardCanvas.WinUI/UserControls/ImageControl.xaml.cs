using ClipboardCanvas.Shared.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.UserControls
{
    public sealed partial class ImageControl : UserControl
    {
        public ImageControl()
        {
            InitializeComponent();
        }

        public IImage? Image
        {
            get => (IImage?)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(nameof(Image), typeof(IImage), typeof(ImageControl), new PropertyMetadata(null));

        public double HeightHint
        {
            get => (double)GetValue(HeightHintProperty);
            set => SetValue(HeightHintProperty, value);
        }
        public static readonly DependencyProperty HeightHintProperty =
            DependencyProperty.Register(nameof(HeightHint), typeof(double), typeof(ImageControl), new PropertyMetadata(16d));

        public double WidthHint
        {
            get => (double)GetValue(WidthHintProperty);
            set => SetValue(WidthHintProperty, value);
        }
        public static readonly DependencyProperty WidthHintProperty =
            DependencyProperty.Register(nameof(WidthHint), typeof(double), typeof(ImageControl), new PropertyMetadata(16d));
    }
}
