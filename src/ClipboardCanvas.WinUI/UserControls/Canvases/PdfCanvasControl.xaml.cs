using ClipboardCanvas.Sdk.ViewModels.Controls.Documents;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.UserControls.Canvases
{
    public sealed partial class PdfCanvasControl : UserControl
    {
        public PdfCanvasControl()
        {
            InitializeComponent();
        }

        public IList<PdfPageViewModel>? PagesSource
        {
            get => (IList<PdfPageViewModel>?)GetValue(PagesSourceProperty);
            set => SetValue(PagesSourceProperty, value);
        }
        public static readonly DependencyProperty PagesSourceProperty =
            DependencyProperty.Register(nameof(PagesSource), typeof(IList<PdfPageViewModel>), typeof(PdfCanvasControl), new PropertyMetadata(null));
    }
}
