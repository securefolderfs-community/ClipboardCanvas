using ClipboardCanvas.Sdk.Services;
using OwlCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.UI.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    public abstract class BaseApplicationService : IApplicationService
    {
        /// <inheritdoc/>
        public abstract string Platform { get; }

        /// <inheritdoc/>
        public abstract Version AppVersion { get; }

        /// <inheritdoc/>
        public virtual string GetSystemVersion()
        {
            if (OperatingSystem.IsWindows())
            {
                var windows = Environment.OSVersion;
                return $"Windows {windows.Version}";
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public abstract Task OpenUriAsync(Uri uri);

        /// <inheritdoc/>
        public abstract Task TryRestartAsync();

        /// <inheritdoc/>
        public abstract Task LaunchHandlerAsync(IFile file, CancellationToken cancellationToken);
    }
}
