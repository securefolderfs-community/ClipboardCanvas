using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.ViewModels.Controls.Canvases;
using ClipboardCanvas.Shared.Enums;
using ClipboardCanvas.Shared.Helpers;
using MimeTypes;
using OwlCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;
using ClipboardCanvas.Shared.Extensions;

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
            await Task.CompletedTask;

            return typeHint switch
            {
                TypeHint.Image => storable is IFile file ? new ImageCanvasViewModel(file, sourceModel).WithInitAsync() : null,
                TypeHint.PlainText => storable is IFile file ? new TextCanvasViewModel(file, sourceModel).WithInitAsync() : null,
                _ => (BaseCanvasViewModel?)null
            } ?? throw new ArgumentOutOfRangeException(nameof(typeHint));
        }
    }
}
