using ClipboardCanvas.Sdk.ViewModels.Controls.Canvases;
using Microsoft.UI.Xaml;

namespace ClipboardCanvas.WinUI.TemplateSelectors
{
    internal sealed class CanvasTemplateSelector : BaseTemplateSelector<BaseCanvasViewModel>
    {
        public DataTemplate? TextCanvasTemplate { get; set; }

        public DataTemplate? ImageCanvasTemplate { get; set; }

        public DataTemplate? PdfCanvasTemplate { get; set; }

        public DataTemplate? VideoCanvasTemplate { get; set; }

        public DataTemplate? CodeCanvasTemplate { get; set; }

        public DataTemplate? FolderCanvasTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? SelectTemplateCore(BaseCanvasViewModel? item, DependencyObject container)
        {
            return item switch
            {
                TextCanvasViewModel => TextCanvasTemplate,
                ImageCanvasViewModel => ImageCanvasTemplate,
                PdfCanvasViewModel => PdfCanvasTemplate,
                VideoCanvasViewModel => VideoCanvasTemplate,
                CodeCanvasViewModel => CodeCanvasTemplate,
                FolderCanvasViewModel => FolderCanvasTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
