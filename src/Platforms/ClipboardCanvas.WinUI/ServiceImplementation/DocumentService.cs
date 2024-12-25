using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.WinUI.AppModels;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Pdf;

namespace ClipboardCanvas.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IDocumentService"/>
    internal sealed class DocumentService : IDocumentService
    {
        /// <inheritdoc/>
        public async Task<IDocument> ReadPdfAsync(Stream pdfStream, CancellationToken cancellationToken)
        {
            var document = await PdfDocument.LoadFromStreamAsync(pdfStream.AsRandomAccessStream()).AsTask(cancellationToken);
            return new PdfFileDocument(document);
        }
    }
}
