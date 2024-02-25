using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.Services
{
    /// <summary>
    /// A service that interacts with the system file explorer.
    /// </summary>
    public interface IFileExplorerService
    {
        /// <summary>
        /// Opens the app folder.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task OpenAppFolderAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Opens provided <paramref name="folder"/> in file explorer.
        /// </summary>
        /// <param name="folder">The folder to open file explorer in.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task OpenInFileExplorerAsync(IFolder folder, CancellationToken cancellationToken = default);

        /// <summary>
        /// Awaits the user input and picks single folder from the file explorer dialog.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. If successful and a folder has been picked, returns <see cref="IFolder"/>; otherwise null.</returns>
        Task<IFolder?> PickFolderAsync(CancellationToken cancellationToken = default);
    }
}
