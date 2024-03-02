using ClipboardCanvas.Shared.ComponentModel;

namespace ClipboardCanvas.WinUI.Imaging
{
    internal sealed class IconImage : IImage
    {
        public string IconGlyph { get; }

        public IconImage(string iconGlyph)
        {
            IconGlyph = iconGlyph;
        }
    }
}
