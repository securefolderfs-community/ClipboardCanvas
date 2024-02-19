using ClipboardCanvas.Shared.ComponentModel;
using Microsoft.UI.Xaml.Controls;

namespace ClipboardCanvas.WinUI.Imaging
{
    internal sealed class IconImage : IImage
    {
        public FontIcon IconGlyph { get; }

        public IconImage(FontIcon iconGlyph)
        {
            IconGlyph = iconGlyph;
        }
    }
}
