using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;

namespace ClipboardCanvas.WinUI.Extensions
{
    public static class ImagingExtensions
    {
        public static async Task<Stream> OpenReadAsync(this BitmapImage bitmapImage, CancellationToken cancellationToken)
        {
            var winrtStreamReference = RandomAccessStreamReference.CreateFromUri(bitmapImage.UriSource);
            var winrtStream = await winrtStreamReference.OpenReadAsync().AsTask(cancellationToken);

            return winrtStream.AsStreamForRead(4096);
        }
    }
}
