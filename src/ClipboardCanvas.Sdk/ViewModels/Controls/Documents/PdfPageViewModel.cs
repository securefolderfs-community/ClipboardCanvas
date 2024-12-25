using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Shared.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Documents
{
    public sealed partial class PdfPageViewModel : ObservableObject, IAsyncInitialize, IDisposable
    {
        private readonly IDocumentPage _documentPage;
        [ObservableProperty] private IImage? _PageImage;
        [ObservableProperty] private ObservableCollection<OcrStringViewModel> _OcrText;

        private ITextRecognitionService TextRecognitionService { get; } = Ioc.Default.GetRequiredService<ITextRecognitionService>();

        public PdfPageViewModel(IDocumentPage documentPage)
        {
            _documentPage = documentPage;
            OcrText = new();
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            PageImage = await _documentPage.AsImageAsync(cancellationToken);
            if (PageImage is null)
                return;

            await foreach (var item in TextRecognitionService.RecognizeAsync(PageImage, cancellationToken))
                OcrText.Add(item);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _documentPage.Dispose();
        }
    }
}
