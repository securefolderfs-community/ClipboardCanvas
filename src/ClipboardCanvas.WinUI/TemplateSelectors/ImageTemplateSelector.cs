using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.WinUI.Imaging;
using Microsoft.UI.Xaml;

namespace ClipboardCanvas.WinUI.TemplateSelectors
{
    internal sealed class ImageTemplateSelector : BaseTemplateSelector<IImage>
    {
        public DataTemplate? IconImageDataTemplate { get; set; }

        protected override DataTemplate? SelectTemplateCore(IImage? item, DependencyObject container)
        {
            return item switch
            {
                IconImage => IconImageDataTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
