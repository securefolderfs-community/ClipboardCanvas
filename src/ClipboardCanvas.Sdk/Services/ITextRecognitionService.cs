using ClipboardCanvas.Sdk.ViewModels.Controls.Documents;
using ClipboardCanvas.Shared.ComponentModel;
using System.Collections.Generic;
using System.Threading;

namespace ClipboardCanvas.Sdk.Services
{
    public interface ITextRecognitionService
    {
        /// <summary>
        /// Recognizes the text from provided <see cref="IImage"/> and returns a collection of recognized words.
        /// </summary>
        /// <param name="image">The image to recognize the text from.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>An instance of <see cref="IAsyncEnumerable{T}"/> of <see cref="OcrStringViewModel"/> that represent recognized words.</returns>
        IAsyncEnumerable<OcrStringViewModel> RecognizeAsync(IImage image, CancellationToken cancellationToken);
    }
}
