using ClipboardCanvas.Sdk.AppModels;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls.Canvases;
using ClipboardCanvas.Shared.Enums;
using ClipboardCanvas.WinUI.Helpers;
using OwlCore.Storage;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="ICanvasService"/>
    internal sealed class CanvasService : ICanvasService
    {
        /// <inheritdoc/>
        public async Task<BaseCanvasViewModel> GetCanvasForStorableAsync(IStorableChild storable, IDataSourceModel sourceModel, CancellationToken cancellationToken)
        {
            var classification = FileTypeHelper.GetClassification(storable);
            var viewModel = classification.TypeHint switch
            {
                TypeHint.Image => storable is IFile file ? new ImageCanvasViewModel(file, sourceModel) : null,
                TypeHint.PlainText => storable is IFile file ? MatchText(file, sourceModel, classification) : null,
                TypeHint.Media => storable is IFile file ? new VideoCanvasViewModel(file, sourceModel) : null,
                TypeHint.Document => storable is IFile file ? MatchDocument(file, sourceModel, classification) : null,

                _ => null,
            } ?? throw new ArgumentOutOfRangeException(nameof(classification.TypeHint)); // TODO: Return fallback canvas

            await viewModel.InitAsync(cancellationToken);
            return viewModel;
        }

        private static BaseCanvasViewModel? MatchText(IFile file, IDataSourceModel sourceModel, TypeClassification classification)
        {
            if (classification.Extension is not null && FileTypeHelper.CodeExtensions.Contains(classification.Extension))
                return new CodeCanvasViewModel(file, sourceModel);

            return new TextCanvasViewModel(file, sourceModel);
        }

        private static BaseCanvasViewModel? MatchDocument(IFile file, IDataSourceModel sourceModel, TypeClassification classification)
        {
            if (classification.MimeType == "application/pdf")
                return new PdfCanvasViewModel(file, sourceModel);

            return null;
        }
    }
}
