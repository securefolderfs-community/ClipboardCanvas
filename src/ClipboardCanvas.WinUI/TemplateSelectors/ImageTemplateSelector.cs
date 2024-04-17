using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.WinUI.Imaging;
using Microsoft.UI.Xaml;

namespace ClipboardCanvas.WinUI.TemplateSelectors
{
    internal sealed class ImageTemplateSelector : BaseTemplateSelector<IImage>
    {
        public DataTemplate? IconImageDataTemplate { get; set; }

        public DataTemplate? ImageBitmapDataTemplate { get; set; }

        public DataTemplate? PdfPageImageDataTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(IImage? item, DependencyObject container)
        {
            return item switch
            {
                IconImage => IconImageDataTemplate,
                ImageBitmap => ImageBitmapDataTemplate,
                PdfPageImage => PdfPageImageDataTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
