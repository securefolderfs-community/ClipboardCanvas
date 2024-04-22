using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Shared.ComponentModel;
using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.Services
{
    /// <summary>
    /// A service used to manage app image assets.
    /// </summary>
    public interface IMediaService
    {
        Task SaveImageAsync(IImage image, IFile destination, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the bitmap from provided <paramref name="file"/> and converts it to <see cref="IImage"/>.
        /// </summary>
        /// <param name="file">The image file to read.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IImage"/> representation of the bitmap file.</returns>
        Task<IImage> ReadImageAsync(IFile file, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves an image based on the contents of the <paramref name="collectionModel"/>.
        /// </summary>
        /// <param name="collectionModel">The data source.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IImage"/> representation of the collection icon.</returns>
        Task<IImage> GetCollectionIconAsync(IDataSourceModel collectionModel, CancellationToken cancellationToken);

        Task<IMediaSource> GetVideoPlaybackAsync(IFile file, CancellationToken cancellationToken);
    }
}
