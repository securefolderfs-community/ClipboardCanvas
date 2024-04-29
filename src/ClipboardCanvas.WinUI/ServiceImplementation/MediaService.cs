using ClipboardCanvas.Sdk.Enums;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.Shared.Enums;
using ClipboardCanvas.WinUI.AppModels;
using ClipboardCanvas.WinUI.Helpers;
using ClipboardCanvas.WinUI.Imaging;
using MimeTypes;
using OwlCore.Storage;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Core;
using IMediaSource = ClipboardCanvas.Sdk.Models.IMediaSource;

namespace ClipboardCanvas.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IMediaService"/>
    internal sealed class MediaService : IMediaService
    {
        /// <inheritdoc/>
        public async Task SaveImageAsync(IImage image, IFile destination, CancellationToken cancellationToken)
        {
            if (image is not ImageBitmap bitmap)
                return;

            await using var stream = await destination.OpenStreamAsync(FileAccess.ReadWrite, cancellationToken);
            using var winrtStream = stream.AsRandomAccessStream();

            var mimeType = MimeTypeMap.GetMimeType(destination.Id);
            var encoderGuid = MimeHelpers.MimeToBitmapEncoder(mimeType);
            var encoder = await BitmapEncoder.CreateAsync(encoderGuid, winrtStream).AsTask(cancellationToken);

            encoder.SetSoftwareBitmap(bitmap.SoftwareBitmap);
            encoder.IsThumbnailGenerated = true;

            try
            {
                await encoder.FlushAsync();
            }
            catch (Exception err)
            {
                const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);
                switch (err.HResult)
                {
                    case WINCODEC_ERR_UNSUPPORTEDOPERATION:
                        // If the encoder does not support writing a thumbnail, then try again
                        // but disable thumbnail generation.
                        encoder.IsThumbnailGenerated = false;
                        break;
                    default:
                        throw;
                }
            }

            if (!encoder.IsThumbnailGenerated)
                await encoder.FlushAsync();
        }

        /// <inheritdoc/>
        public async Task<IImage> ReadImageAsync(IFile file, CancellationToken cancellationToken)
        {
            await using var stream = await file.OpenStreamAsync(FileAccess.Read, cancellationToken);
            using var winrtStream = stream.AsRandomAccessStream();

            var mimeType = MimeTypeMap.GetMimeType(file.Id);
            return await ImagingHelpers.GetBitmapFromStreamAsync(winrtStream, mimeType, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IImage> GetCollectionIconAsync(IDataSourceModel collectionModel, CancellationToken cancellationToken)
        {
            var unclassified = 0u;
            var documents = 0u;
            var images = 0u;
            var media = 0u;
            var audio = 0u;

            await foreach (var item in collectionModel.Source.GetItemsAsync(StorableType.All, cancellationToken))
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

        public IImage? GetIcon(IconType iconType)
        {
            var glyph = iconType switch
            {
                IconType.Share => "\uE72D",
                _ => null
            };

            if (glyph is null)
                return null;

            return new IconImage(glyph);
        }
    }
}
