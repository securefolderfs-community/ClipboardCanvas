using ClipboardCanvas.Sdk.Services;
using OwlCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;

namespace ClipboardCanvas.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    internal sealed class ApplicationService : IApplicationService
    {
        /// <inheritdoc/>
        public async Task LaunchHandlerAsync(IFile file, CancellationToken cancellationToken)
        {
            var uri = new Uri(file.Id);
            await Launcher.LaunchUriAsync(uri).AsTask(cancellationToken);
        }
    }
}
