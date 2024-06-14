using ClipboardCanvas.Shared.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;

namespace ClipboardCanvas.WinUI.Imaging
{
    /// <inheritdoc cref="IImage"/>
    public sealed class ImageBitmap : IImage
    {
        public SoftwareBitmap SoftwareBitmap { get; }

        public BitmapImage Source { get; }

        public ImageBitmap(BitmapImage source, SoftwareBitmap softwareBitmap)
        {
            Source = source;
            SoftwareBitmap = softwareBitmap;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            SoftwareBitmap?.Dispose();
        }
    }
}