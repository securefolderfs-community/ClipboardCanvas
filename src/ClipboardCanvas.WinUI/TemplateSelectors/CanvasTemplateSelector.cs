using ClipboardCanvas.Sdk.ViewModels.Controls.Canvases;
using Microsoft.UI.Xaml;

namespace ClipboardCanvas.WinUI.TemplateSelectors
{
    internal sealed class CanvasTemplateSelector : BaseTemplateSelector<BaseCanvasViewModel>
    {
        public DataTemplate? TextCanvasTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? SelectTemplateCore(BaseCanvasViewModel? item, DependencyObject container)
        {
            return item switch
            {
                TextCanvasViewModel => TextCanvasTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
