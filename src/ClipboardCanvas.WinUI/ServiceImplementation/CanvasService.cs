using ClipboardCanvas.Sdk.AppModels;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls.Canvases;
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
            var classification = new TypeClassification(mimeType, Path.GetExtension(storable.Id)); // TODO

            if (mimeType.StartsWith("image/"))
                return await ImageCanvasAsync(storable, sourceModel, cancellationToken);

            if (mimeType.StartsWith("text/"))
                return await TextCanvasAsync(storable, sourceModel, cancellationToken);

            throw new NotImplementedException();
            //return new DefaultCanvasViewModel();
        }

        private async Task<BaseCanvasViewModel> ImageCanvasAsync(IStorableChild storable, IDataSourceModel sourceModel, CancellationToken cancellationToken)
        {
            return new ImageCanvasViewModel(sourceModel);
        }

        private async Task<BaseCanvasViewModel> TextCanvasAsync(IStorableChild storable, IDataSourceModel sourceModel, CancellationToken cancellationToken)
        {
            return new TextCanvasViewModel(sourceModel);
        }
    }
}
