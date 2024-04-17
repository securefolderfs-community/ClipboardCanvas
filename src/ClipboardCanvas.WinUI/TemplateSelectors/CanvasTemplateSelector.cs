﻿using ClipboardCanvas.Sdk.ViewModels.Controls.Canvases;
using Microsoft.UI.Xaml;

namespace ClipboardCanvas.WinUI.TemplateSelectors
{
    internal sealed class CanvasTemplateSelector : BaseTemplateSelector<BaseCanvasViewModel>
    {
        public DataTemplate? TextCanvasTemplate { get; set; }

        public DataTemplate? ImageCanvasTemplate { get; set; }

        public DataTemplate? PdfCanvasTemplate { get; set; }

        /// <inheritdoc/>
        protected override DataTemplate? SelectTemplateCore(BaseCanvasViewModel? item, DependencyObject container)
        {
            return item switch
            {
                TextCanvasViewModel => TextCanvasTemplate,
                ImageCanvasViewModel => ImageCanvasTemplate,
                PdfCanvasViewModel => PdfCanvasTemplate,
                _ => base.SelectTemplateCore(item, container)
            };
        }
    }
}
