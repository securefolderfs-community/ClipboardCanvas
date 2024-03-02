using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Shared.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.Services
{
    /// <summary>
    /// A service used to manage app image assets.
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Retrieves an image based on the contents of the <paramref name="collectionModel"/>.
        /// </summary>
        /// <param name="collectionModel">The data source.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IImage"/> representation of the collection icon.</returns>
        Task<IImage> GetCollectionIconAsync(ICanvasSourceModel collectionModel, CancellationToken cancellationToken);
    }
}
