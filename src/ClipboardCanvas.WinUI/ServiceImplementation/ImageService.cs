using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.Shared.Enums;
using ClipboardCanvas.Shared.Helpers;
using ClipboardCanvas.WinUI.Imaging;
using Microsoft.UI.Xaml.Controls;
using OwlCore.Storage;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IImageService"/>
    internal sealed class ImageService : IImageService
    {
        /// <inheritdoc/>
        public async Task<IImage> GetCollectionIconAsync(ICanvasSourceModel collectionModel, CancellationToken cancellationToken)
        {
            var unclassified = 0u;
            var documents = 0u;
            var images = 0u;
            var media = 0u;
            var audio = 0u;

            await foreach (var item in collectionModel.GetItemsAsync(StorableType.All, cancellationToken))
            {
                var fileType = FileExtensionHelper.GetFileType(Path.GetExtension(item.Name));
                switch (fileType)
                {
                    case FileType.Unclassified:
                        unclassified++;
                        break;

                    case FileType.Document:
                        documents++;
                        break;

                    case FileType.Image:
                        images++;
                        break;

                    case FileType.Media:
                        media++;
                        break;

                    case FileType.Audio:
                        audio++;
                        break;
                }
            }

            ulong total = unclassified + documents + images + media + audio;
            if (total == 0UL)
                return new IconImage(new FontIcon() { Glyph = "\uF0E2" });

            var unclassified2 = (unclassified * 100f) / total;
            var documents2 = (documents * 100f) / total;
            var images2 = (images * 100f) / total;
            var media2 = (media * 100f) / total;
            var audio2 = (audio * 100f) / total;

            var glyph = string.Empty;

            if (IsMajority(unclassified2, documents2, images2, media2, audio2)) glyph = "\uF0E2";
            else if (IsMajority(documents2, unclassified2, images2, media2, audio2)) glyph = "\uE8A5";
            else if (IsMajority(images2, unclassified2, documents2, media2, audio2)) glyph = "\uE91B";
            else if (IsMajority(media2, unclassified2, documents2, images2, audio2)) glyph = "\uE714";
            else if (IsMajority(audio2, unclassified2, documents2, images2, media2)) glyph = "\uE8D6";
            else glyph = "\uF0E2";

            return new IconImage(new FontIcon() { Glyph = glyph });
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
