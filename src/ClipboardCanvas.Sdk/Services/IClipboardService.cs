using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.Shared.Enums;
using OwlCore.Storage;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.Services
{
    /// <summary>
    /// Represents a service for interacting with the clipboard.
    /// </summary>
    public interface IClipboardService : INotifyCollectionChanged
    {
        /// <summary>
        /// Gets the current content of the clipboard.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous operation.
        /// If clipboard is not empty, returns the current clipboard content represented by <see cref="IClipboardData"/>; otherwise null.
        /// </returns>
        Task<IClipboardData?> GetContentAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Sets the clipboard content to the specified <paramref name="image"/>.
        /// </summary>
        /// <param name="image">The image to set in the clipboard.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SetImageAsync(IImage image, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the clipboard content to the specified <paramref name="text"/>.
        /// </summary>
        /// <param name="text">The text to set in the clipboard.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SetTextAsync(string text, CancellationToken cancellationToken);

        /// <summary>
        /// Sets the clipboard content to the specified <paramref name="storage"/>.
        /// </summary>
        /// <param name="storage">The collection of storage to set in the clipboard.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task SetStorageAsync(IEnumerable<IStorableChild> storage, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Represents a clipboard data object.
    /// </summary>
    public interface IClipboardData
    {
        /// <summary>
        /// Gets the type of the clipboard data.
        /// </summary>
        ContentType PrimaryType { get; }

        /// <summary>
        /// Gets the text that is contained within the clipboard data.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="string"/> that represents the clipboard data.</returns>
        Task<string> GetTextAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the image that is contained within the clipboard data.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="IImage"/> that represents the clipboard data.</returns>
        Task<IImage> GetImageAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the stream that contains the clipboard data.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="Stream"/> that represents the clipboard data.</returns>
        Task<Stream> OpenReadAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets a collection of storage items in the clipboard data.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>Returns an async operation represented by <see cref="IAsyncEnumerable{T}"/> of type <see cref="IStorable"/> for storage items found in the clipboard data.</returns>
        IAsyncEnumerable<IStorable> GetStorageAsync(CancellationToken cancellationToken);
    }
}
