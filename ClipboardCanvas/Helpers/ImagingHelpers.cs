using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

using ClipboardCanvas.Helpers.Filesystem;
using Windows.UI.Xaml.Media;

namespace ClipboardCanvas.Helpers
{
    public static class ImagingHelpers
    {
        public static async Task<IRandomAccessStream> GetTransparentThumbnail(this StorageFile file, ThumbnailMode mode, uint requestedSize, ThumbnailOptions options = ThumbnailOptions.None)
        {
            if (FileHelpers.IsPathEqualExtension(file.Path, ".png"))
            {
                // Try to create a scaled-down version of the PNG with transparency
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    IRandomAccessStream thumbnail = new InMemoryRandomAccessStream();
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, thumbnail);

                    SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                    encoder.SetSoftwareBitmap(softwareBitmap);

                    encoder.BitmapTransform.ScaledHeight = requestedSize;
                    encoder.BitmapTransform.ScaledWidth = (uint)(requestedSize * 1.7777d);
                    encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;

                    await encoder.FlushAsync();
                    await thumbnail.FlushAsync();

                    softwareBitmap.Dispose();
                    thumbnail.Seek(0);

                    return thumbnail;
                }
            }

            return await file.GetThumbnailAsync(mode, requestedSize, options);
        }

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

        public static async Task<BitmapImage> ToBitmapAsync(IRandomAccessStream stream)
        {
            if (stream == null)
            {
                return null;
            }

            stream.Seek(0);
            BitmapImage image = new BitmapImage();
            await image.SetSourceAsync(stream);

            return image;
        }

        public static async Task<ImageSource> ToImageAsync(Uri uri)
        {
            ImageSource image = null;
            try
            {
                IRandomAccessStreamReference uriStream = RandomAccessStreamReference.CreateFromUri(uri);

                using (IRandomAccessStream stream = await uriStream.OpenReadAsync())
                {
                    if (uri.AbsoluteUri.EndsWith(".svg"))
                    {
                        image = new SvgImageSource(uri);
                    }
                    else
                    {
                        image = new BitmapImage();
                        await (image as BitmapImage).SetSourceAsync(stream);
                    }
                }
            }
            catch
            {
            }

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
    }
}
