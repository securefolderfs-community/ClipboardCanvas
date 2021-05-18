using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;

namespace ClipboardCanvas.Helpers
{
    public static class ImagingHelpers
    {
        public static async Task<BitmapImage> ToBitmapAsync(this byte[] data)
        {
            if (data == null)
            {
                return null;
            }

            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                BitmapImage image = new BitmapImage();
                await image.SetSourceAsync(memoryStream.AsRandomAccessStream());

                return image;
            }
        }

        public static async Task<BitmapImage> ToBitmapAsync(Stream stream)
        {
            if (stream == null)
            {
                return null;
            }

            stream.Position = 0;
            BitmapImage image = new BitmapImage();
            await image.SetSourceAsync(stream.AsRandomAccessStream());

            return image;
        }

        public static async Task<byte[]> GetBytesFromSoftwareBitmap(this SoftwareBitmap softwareBitmap, Guid encoderId)
        {
            byte[] array = null;

            // First: Use an encoder to copy from SoftwareBitmap to an in-mem stream (FlushAsync)
            // Next:  Use ReadAsync on the in-mem stream to get byte[] array

            using (InMemoryRandomAccessStream imras = new InMemoryRandomAccessStream())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(encoderId, imras);
                encoder.SetSoftwareBitmap(softwareBitmap);

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception ex) 
                {
                    Console.WriteLine(ex);
                    return new byte[0];
                }

                array = new byte[imras.Size];
                await imras.ReadAsync(array.AsBuffer(), (uint)imras.Size, InputStreamOptions.None);
            }

            return array;
        }

        public static async Task<BitmapImage> GetImageFromURL(string url)
        {
            using (IRandomAccessStreamWithContentType stream = await RandomAccessStreamReference.CreateFromUri(new Uri(url)).OpenReadAsync())
            {
                BitmapImage bitampImage = new BitmapImage();

                bitampImage.SetSource(stream);

                return bitampImage;
            }
        }
    }
}
