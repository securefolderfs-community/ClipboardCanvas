using OwlCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Sdk.Services
{
    /// <summary>
    /// A service that interacts with common app-related APIs.
    /// </summary>
    public interface IApplicationService
    {
        /// <summary>
        /// Gets the name that uniquely identifies a platform.
        /// </summary>
        string Platform { get; }

        /// <summary>
        /// Gets the version of the app.
        /// </summary>
        Version AppVersion { get; }

        /// <summary>
        /// Gets the version information of the platform that the app is running on.
        /// </summary>
        /// <remarks>
        /// The return value may contain information like OS build version, release number, and other platform-specific members.
        /// </remarks>
        /// <returns>A <see cref="string"/> containing version data.</returns>
        string GetSystemVersion();

        /// <summary>
        /// Launches an URI from app. This can be an URL, folder path, etc.
        /// </summary>
        /// <param name="uri">The URI to launch.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task OpenUriAsync(Uri uri);

        /// <summary>
        /// Tries to schedule the application for restart.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task TryRestartAsync();

        /// <summary>
        /// Launches the associated app to handle the file being opened.
        /// </summary>
        /// <param name="file">The file to open in a handler.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task LaunchHandlerAsync(IFile file, CancellationToken cancellationToken);
    }
}
