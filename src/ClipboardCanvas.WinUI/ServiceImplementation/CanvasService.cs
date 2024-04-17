using ClipboardCanvas.Sdk.AppModels;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls.Canvases;
using ClipboardCanvas.Shared.Enums;
using ClipboardCanvas.Shared.Helpers;
using MimeTypes;
using OwlCore.Storage;
using System;
using System.IO;
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
            var mimeType = MimeTypeMap.GetMimeType(storable.Id);
            var typeHint = FileExtensionHelper.GetTypeFromMime(mimeType);
            var classification = new TypeClassification(mimeType, typeHint, Path.GetExtension(storable.Id));

            var viewModel = typeHint switch
            {
                TypeHint.Image => storable is IFile file ? new ImageCanvasViewModel(file, sourceModel) : null,
                TypeHint.PlainText => storable is IFile file ? new TextCanvasViewModel(file, sourceModel) : null,
                TypeHint.Document => MatchDocument(storable, sourceModel, classification),

                _ => MatchText(storable, sourceModel, classification)
            } ?? throw new ArgumentOutOfRangeException(nameof(typeHint)); // TODO: Return fallback canvas

            await viewModel.InitAsync(cancellationToken);
            return viewModel;
        }

        private static BaseCanvasViewModel? MatchText(IStorableChild storable, IDataSourceModel sourceModel, TypeClassification classification)
        {
            _ = storable;
            _ = sourceModel;
            _ = classification;

            // TODO: Read arbitrary file into a text canvas, if possible
            return null;
        }

        private static BaseCanvasViewModel? MatchDocument(IStorableChild storable, IDataSourceModel sourceModel, TypeClassification classification)
        {
            if (classification.MimeType == "application/pdf" && storable is IFile pdfFile)
                return new PdfCanvasViewModel(pdfFile, sourceModel);

            return null;
        }
    }
}
