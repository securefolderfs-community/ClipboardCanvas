using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls.Documents;
using ClipboardCanvas.Shared.Extensions;
using CommunityToolkit.Mvvm.DependencyInjection;
using OwlCore.Storage;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Canvases
{
    public class PdfCanvasViewModel : BaseCanvasViewModel
    {
        /// <summary>
        /// Gets the collection of displayable pages.
        /// </summary>
        public ObservableCollection<PdfPageViewModel> Pages { get; }

        private IDocumentService DocumentService { get; } = Ioc.Default.GetRequiredService<IDocumentService>();

        public PdfCanvasViewModel(IFile pdfFile, IDataSourceModel sourceModel)
            : base(pdfFile, sourceModel)
        {
            Pages = new();
        }

        public PdfCanvasViewModel(IEnumerable<PdfPageViewModel> pages, IDataSourceModel sourceModel)
            : base(sourceModel)
        {
            Pages = new(pages);
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (Storable is not IFile pdfFile || !Pages.IsEmpty())
                return;

            await using var pdfStream = await pdfFile.OpenStreamAsync(FileAccess.Read, cancellationToken);
            using var pdfDocument = await DocumentService.ReadPdfAsync(pdfStream, cancellationToken);

            // TODO: Load pages dynamically
            await foreach (var item in pdfDocument.GetPagesAsync(cancellationToken))
            {
                var pageViewModel = new PdfPageViewModel(item);
                await pageViewModel.InitAsync(cancellationToken);

                Pages.Add(pageViewModel);
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            lock (Pages)
                Pages.DisposeElements();
        }
    }
}
