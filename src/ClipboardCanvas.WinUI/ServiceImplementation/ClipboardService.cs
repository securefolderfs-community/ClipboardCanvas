using ClipboardCanvas.Sdk.AppModels;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.Shared.Enums;
using ClipboardCanvas.WinUI.Extensions;
using ClipboardCanvas.WinUI.Imaging;
using ClipboardCanvas.WinUI.Storage;
using OwlCore.Storage;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;

namespace ClipboardCanvas.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IClipboardService"/>
    internal sealed class ClipboardService : IClipboardService
    {
        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        
        /// <inheritdoc/>
        public Task<IClipboardData?> GetContentAsync(CancellationToken cancellationToken)
        {
            var data = Clipboard.GetContent();
            if (data == null)
                return Task.FromResult<IClipboardData?>(null);

            return Task.FromResult<IClipboardData?>(ClipboardData.Import(data));
        }

        /// <inheritdoc/>
        public async Task SetImageAsync(IImage image, CancellationToken cancellationToken)
        {
            var dataPackage = new DataPackage();
            if (image is ImageBitmap bitmapImage)
            {
                var stream = await bitmapImage.Source.OpenReadAsync(cancellationToken);
                dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream.AsRandomAccessStream()));
            }

            Clipboard.SetContent(dataPackage);
        }

        /// <inheritdoc/>
        public Task SetTextAsync(string text, CancellationToken cancellationToken)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SetStorageAsync(IEnumerable<IStorableChild> storage, CancellationToken cancellationToken)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            var dataPackage = new DataPackage();
            foreach (var item in storage)
            {
                if (item is WindowsStorageFolder folder)
                    dataPackage.SetStorageItems(new IStorageItem[] { folder.storage });

                if (item is WindowsStorageFile file)
                    dataPackage.SetStorageItems(new IStorageItem[] { file.storage });
            }

            return Task.CompletedTask;
        }
    }

    /// <inheritdoc cref="IClipboardData"/>
    internal sealed class ClipboardData : IClipboardData
    {
        private readonly DataPackageView _dataPackage;

        /// <inheritdoc/>
        public TypeClassification Classification { get; }

        public ClipboardData(DataPackageView dataPackage, TypeClassification classification)
        {
            _dataPackage = dataPackage;
            Classification = classification;
        }

        public Task<string> GetTextAsync(CancellationToken cancellationToken)
        {
            if (Classification.TypeHint == TypeHint.PlainText)
                return _dataPackage.GetTextAsync().AsTask(cancellationToken);

            return Task.FromResult(string.Empty);
        }

        public async Task<IImage> GetImageAsync(CancellationToken cancellationToken)
        {
            if (Classification.TypeHint == TypeHint.Image)
            {
                var winrtStreamReference = await _dataPackage.GetBitmapAsync().AsTask(cancellationToken);
                var winrtStream = await winrtStreamReference.OpenReadAsync().AsTask(cancellationToken);

                return new ImageBitmap(winrtStream.AsStream());
            }

            throw new InvalidOperationException("Could not retrieve image.");
        }

        public async Task<Stream> OpenReadAsync(CancellationToken cancellationToken)
        {
            switch (Classification.TypeHint)
            {
                case TypeHint.Storage:
                {
                    var files = await GetStorageAsync(cancellationToken).ToListAsync(cancellationToken);
                    var storable = files.FirstOrDefault();
                    if (storable is IFile file)
                        return await file.OpenStreamAsync(FileAccess.Read, cancellationToken);

                    throw new InvalidOperationException("The item is not a file.");
                }

                case TypeHint.Image:
                {
                    var image = await GetImageAsync(cancellationToken);
                    if (image is ImageBitmap bitmap)
                        return await bitmap.Source.OpenReadAsync(cancellationToken);

                    throw new FormatException("Could not read the bitmap.");
                }

                case TypeHint.PlainText:
                case TypeHint.Document:
                    throw new NotImplementedException();

                case TypeHint.Media:
                case TypeHint.Audio:
                    throw new NotImplementedException();

                default:
                case TypeHint.Unclassified:
                    throw new InvalidOperationException("Could not open a stream to clipboard data.");
            }
        }

        public async IAsyncEnumerable<IStorable> GetStorageAsync(CancellationToken cancellationToken)
        {
            if (Classification.TypeHint == TypeHint.Storage)
            {
                var storageItems = await _dataPackage.GetStorageItemsAsync().AsTask(cancellationToken);
                foreach (var item in storageItems)
                {
                    yield return item switch
                    {
                        StorageFile file => new WindowsStorageFile(file),
                        StorageFolder folder => new WindowsStorageFolder(folder),
                        _ => throw new NotSupportedException()
                    };
                }
            }
        }

        public static ClipboardData Import(DataPackageView dataPackage)
        {
            if (dataPackage.Contains(StandardDataFormats.Text))
                return new ClipboardData(dataPackage, new TypeClassification("text/plain", TypeHint.PlainText));

            if (dataPackage.Contains(StandardDataFormats.Bitmap))
                return new ClipboardData(dataPackage, new TypeClassification("image/png", TypeHint.Image));

            if (dataPackage.Contains(StandardDataFormats.StorageItems))
                return new ClipboardData(dataPackage, new TypeClassification("application/octet-stream", TypeHint.Storage));

            return new ClipboardData(dataPackage, new TypeClassification("application/octet-stream", TypeHint.Unclassified));
        }
    }
}