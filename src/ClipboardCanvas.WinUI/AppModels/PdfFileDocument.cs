using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.WinUI.Imaging;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace ClipboardCanvas.WinUI.AppModels
{
    /// <inheritdoc cref="IDocument"/>
    internal sealed class PdfFileDocument : IDocument
    {
        private readonly PdfDocument _pdfDocument;

        /// <inheritdoc/>
        public int PageCount => (int)_pdfDocument.PageCount;

        public PdfFileDocument(PdfDocument pdfDocument)
        {
            _pdfDocument = pdfDocument;
        }

        /// <inheritdoc/>
        public Task<IDocumentPage> GetPageAsync(int index, CancellationToken cancellationToken)
        {
            var page = _pdfDocument.GetPage((uint)index);
            return Task.FromResult<IDocumentPage>(new PdfDocumentPage(page));
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IDocumentPage> GetPagesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            for (var i = 0u; i < _pdfDocument.PageCount; i++)
            {
                var page = _pdfDocument.GetPage(i);
                yield return new PdfDocumentPage(page);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }

    /// <inheritdoc cref="IDocumentPage"/>
    internal sealed class PdfDocumentPage : IDocumentPage
    {
        private readonly PdfPage _pdfPage;
        private IImage? _cachedImage;

        public PdfDocumentPage(PdfPage pdfPage)
        {
            _pdfPage = pdfPage;
        }

        /// <inheritdoc/>
        public async Task<IImage?> AsImageAsync(CancellationToken cancellationToken)
        {
            if (_cachedImage is not null)
                return _cachedImage;

            using var memStream = new InMemoryRandomAccessStream();
            await _pdfPage.RenderToStreamAsync(memStream).AsTask(cancellationToken);

            var decoder = await BitmapDecoder.CreateAsync(memStream).AsTask(cancellationToken);
            var softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied).AsTask(cancellationToken);
            
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(softwareBitmap).AsTask(cancellationToken);

            return _cachedImage = new PdfPageImage(source, softwareBitmap);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // _cachedImage is not disposed here since it's the consumer
            // responsibility to dispose of the resource they requested
            _pdfPage.Dispose();
        }
    }
}
