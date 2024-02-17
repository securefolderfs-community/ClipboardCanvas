using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.UserControls
{
    public sealed partial class DynamicRibbon : UserControl
    {
        public DynamicRibbon()
        {
            InitializeComponent();
        }

        public string? ToolBarTitle
        {
            get => (string?)GetValue(ToolBarTitleProperty);
            set => SetValue(ToolBarTitleProperty, value);
        }
        public static readonly DependencyProperty ToolBarTitleProperty =
            DependencyProperty.Register(nameof(ToolBarTitle), typeof(string), typeof(DynamicRibbon), new PropertyMetadata(null));
    }
}
