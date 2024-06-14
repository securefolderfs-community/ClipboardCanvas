using ClipboardCanvas.Shared.ComponentModel;

namespace ClipboardCanvas.WinUI.Imaging
{
    /// <inheritdoc cref="IImage"/>
    internal sealed class IconImage : IImage
    {
        public string IconGlyph { get; }

        public IconImage(string iconGlyph)
        {
            IconGlyph = iconGlyph;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
