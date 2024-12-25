using ClipboardCanvas.Shared.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.Services
{
    public interface IDocumentService
    {
        /// <summary>
        /// Reads the PDF file and translates it into <see cref="IDocument"/>.
        /// </summary>
        /// <param name="pdfStream">The source <see cref="Stream"/> of the PDF file.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A new instance of <see cref="IDocument"/> that represents the PDF file.</returns>
        Task<IDocument> ReadPdfAsync(Stream pdfStream, CancellationToken cancellationToken);
    }
}