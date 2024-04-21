using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.Shared.Enums;
using ClipboardCanvas.WinUI.AppModels;
using ClipboardCanvas.WinUI.Helpers;
using ClipboardCanvas.WinUI.Imaging;
using Microsoft.UI.Xaml.Media.Imaging;
using MimeTypes;
using OwlCore.Storage;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Core;
using Windows.Storage.Streams;
using IMediaSource = ClipboardCanvas.Sdk.Models.IMediaSource;

namespace ClipboardCanvas.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IMediaService"/>
    internal sealed class MediaService : IMediaService
    {
        /// <inheritdoc/>
        public async Task<IImage> ReadImageAsync(IFile file, CancellationToken cancellationToken)
        {
            await using var stream = await file.OpenStreamAsync(FileAccess.Read, cancellationToken);
            using var winrtStream = stream.AsRandomAccessStream();
            using var memStream = new InMemoryRandomAccessStream();

            var mimeType = MimeTypeMap.GetMimeType(file.Id);
            var decoderGuid = MimeHelpers.MimeToBitmapDecoder(mimeType);
            var encoderGuid = MimeHelpers.MimeToBitmapEncoder(mimeType);

            var decoder = await BitmapDecoder.CreateAsync(decoderGuid, winrtStream);
            var encoder = await BitmapEncoder.CreateAsync(encoderGuid, memStream);

            using var softwareBitmap = await decoder.GetSoftwareBitmapAsync().AsTask(cancellationToken);
            encoder.SetSoftwareBitmap(softwareBitmap);

            await encoder.FlushAsync().AsTask(cancellationToken);

            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(memStream).AsTask(cancellationToken);

            return new ImageBitmap(bitmap);
        }

        /// <inheritdoc/>
        public async Task<IImage> GetCollectionIconAsync(IDataSourceModel collectionModel, CancellationToken cancellationToken)
        {
            var unclassified = 0u;
            var documents = 0u;
            var images = 0u;
            var media = 0u;
            var audio = 0u;

            await foreach (var item in collectionModel.GetItemsAsync(StorableType.All, cancellationToken))
            {
                var typeHint = FileTypeHelper.GetType(item);
                switch (typeHint)
                {
                    case TypeHint.Unclassified:
                        unclassified++;
                        break;

                    case TypeHint.Document:
                    case TypeHint.PlainText:
                        documents++;
                        break;

                    case TypeHint.Image:
                        images++;
                        break;

                    case TypeHint.Media:
                        media++;
                        break;

                    case TypeHint.Audio:
                        audio++;
                        break;
                }
            }

            ulong total = unclassified + documents + images + media + audio;
            if (total == 0UL)
                return new IconImage("\uF0E2");

            var unclassified2 = (unclassified * 100f) / total;
            var documents2 = (documents * 100f) / total;
            var images2 = (images * 100f) / total;
            var media2 = (media * 100f) / total;
            var audio2 = (audio * 100f) / total;

            string glyph;
            if (IsMajority(unclassified2, documents2, images2, media2, audio2)) glyph = "\uF0E2";
            else if (IsMajority(documents2, unclassified2, images2, media2, audio2)) glyph = "\uE8A5";
            else if (IsMajority(images2, unclassified2, documents2, media2, audio2)) glyph = "\uE91B";
            else if (IsMajority(media2, unclassified2, documents2, images2, audio2)) glyph = "\uE714";
            else if (IsMajority(audio2, unclassified2, documents2, images2, media2)) glyph = "\uE8D6";
            else glyph = "\uF0E2";

            return new IconImage(glyph);
        }

        /// <inheritdoc/>
        public async Task<IMediaSource> GetVideoPlaybackAsync(IFile file, CancellationToken cancellationToken)
        {
            var stream = await file.OpenStreamAsync(FileAccess.Read, cancellationToken);
            var mime = MimeTypeMap.GetMimeType(file.Id);

            var mediaSource = MediaSource.CreateFromStream(stream.AsRandomAccessStream(), mime);
            return new VideoSource(mediaSource, stream);
        }

        private static bool IsMajority(float first, params float[] other)
        {
            if (first >= 50f)
                return true;

            foreach (var item in other)
            {
                if (item > first)
                    return false;
            }

            return true;
        }
    }
}
