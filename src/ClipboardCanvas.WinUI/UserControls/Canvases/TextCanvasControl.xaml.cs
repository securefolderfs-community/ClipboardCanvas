using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
namespace ClipboardCanvas.WinUI.UserControls.Canvases
{
    public sealed partial class TextCanvasControl : UserControl
    {
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextCanvasControl), new PropertyMetadata(string.Empty));

        public TextCanvasControl()
        {
            this.InitializeComponent();
        }
    }
}
