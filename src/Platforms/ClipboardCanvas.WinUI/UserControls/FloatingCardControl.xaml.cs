using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.UserControls
{
    [ContentProperty(Name = nameof(CardContent))]
    public sealed partial class FloatingCardControl : UserControl
    {
        public FloatingCardControl()
        {
            InitializeComponent();
        }

        public object? CardContent
        {
            get => (object?)GetValue(CardContentProperty);
            set => SetValue(CardContentProperty, value);
        }
        public static readonly DependencyProperty CardContentProperty =
            DependencyProperty.Register(nameof(CardContent), typeof(object), typeof(FloatingCardControl), new PropertyMetadata(null));
    }
}
