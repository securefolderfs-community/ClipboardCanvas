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
            var viewModel = (storable is IFile file ? classification.TypeHint switch
            {
                TypeHint.Image => new ImageCanvasViewModel(file, sourceModel),
                TypeHint.PlainText => MatchText(file, sourceModel, classification),
                TypeHint.Media => new VideoCanvasViewModel(file, sourceModel),
                TypeHint.Document => MatchDocument(file, sourceModel, classification),
                _ => null,

            } : MatchAlternative(storable, sourceModel, classification))

                // TODO: Return fallback canvas
                ?? throw new ArgumentOutOfRangeException(nameof(classification.TypeHint));

            // Initialize and return
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

        private static BaseCanvasViewModel? MatchAlternative(IStorable storable, IDataSourceModel sourceModel, TypeClassification classification)
        {
            if (storable is not IFolder folder)
                return null;

            // TODO: Maybe match to Infinite Canvas as well?
            _ = classification;
            return new FolderCanvasViewModel(folder, sourceModel);
        }
    }
}
