using ClipboardCanvas.Shared.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;

namespace ClipboardCanvas.WinUI.Imaging
{
    /// <inheritdoc cref="IImage"/>
    internal sealed class PdfPageImage : IImage
    {
        public SoftwareBitmapSource ImageSource { get; }

        public SoftwareBitmap SoftwareBitmap { get; }

        public PdfPageImage(SoftwareBitmapSource imageSource, SoftwareBitmap softwareBitmap)
        {
            ImageSource = imageSource;
            SoftwareBitmap = softwareBitmap;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ImageSource.Dispose();
            SoftwareBitmap.Dispose();
        }
    }
}
