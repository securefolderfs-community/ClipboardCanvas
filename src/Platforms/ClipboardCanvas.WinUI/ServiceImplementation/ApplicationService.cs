using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.UI.ServiceImplementation;
using OwlCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;

namespace ClipboardCanvas.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IApplicationService"/>
    internal sealed class ApplicationService : BaseApplicationService
    {
        /// <inheritdoc/>
        public override string Platform { get; } = "Windows";

        /// <inheritdoc/>
        public override Version AppVersion
        {
            get
            {
                var packageVersion = Package.Current.Id.Version;
                return new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
            }
        }

        /// <inheritdoc/>
        public override async Task OpenUriAsync(Uri uri)
        {
            await Launcher.LaunchUriAsync(uri);
        }

        /// <inheritdoc/>
        public override Task TryRestartAsync()
        {
            Microsoft.Windows.AppLifecycle.AppInstance.Restart("/RestartCalled");
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override async Task LaunchHandlerAsync(IFile file, CancellationToken cancellationToken)
        {
            var uri = new Uri(file.Id);
            await Launcher.LaunchUriAsync(uri).AsTask(cancellationToken);
        }
    }
}
