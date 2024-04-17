using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls.Documents;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.WinUI.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.Media.Ocr;

namespace ClipboardCanvas.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="ITextRecognitionService"/>
    internal sealed class OcrService : ITextRecognitionService
    {
        /// <inheritdoc/>
        public async IAsyncEnumerable<OcrStringViewModel> RecognizeAsync(IImage image, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (image is not PdfPageImage pdfImage)
                yield break;

            var ocr = OcrEngine.TryCreateFromUserProfileLanguages();
            if (ocr is null)
                yield break;

            var result = await ocr.RecognizeAsync(pdfImage.SoftwareBitmap).AsTask(cancellationToken);
            foreach (var line in result.Lines)
            {
                var firstWord = line.Words.First();
                var rectangle = new Rectangle((int)firstWord.BoundingRect.X, (int)firstWord.BoundingRect.Y, (int)firstWord.BoundingRect.Width, (int)firstWord.BoundingRect.Height);

                yield return new OcrStringViewModel(line.Text, rectangle);
            }
        }
    }
}
