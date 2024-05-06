using ClipboardCanvas.Sdk.Services;
using ClipboardCanvas.Sdk.Services.Settings;
using ClipboardCanvas.UI.ServiceImplementation.Settings;
using OwlCore.Storage;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.UI.ServiceImplementation
{
    /// <inheritdoc cref="ISettingsService"/>
    public sealed class SettingsService : ISettingsService
    {
        /// <inheritdoc/>
        public IAppSettings AppSettings { get; }

        /// <inheritdoc/>
        public IUserSettings UserSettings { get; }

        public SettingsService(IModifiableFolder settingsFolder)
        {
            AppSettings = new AppSettings(settingsFolder);
            UserSettings = new UserSettings(settingsFolder);
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.WhenAll(AppSettings.InitAsync(cancellationToken), UserSettings.InitAsync(cancellationToken));
        }

        /// <inheritdoc/>
        public Task SaveAsync(CancellationToken cancellationToken = default)
        {
            return Task.WhenAll(AppSettings.SaveAsync(cancellationToken), UserSettings.SaveAsync(cancellationToken));
        }
    }
}
