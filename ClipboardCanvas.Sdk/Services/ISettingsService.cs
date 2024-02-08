using ClipboardCanvas.Sdk.Services.Settings;
using ClipboardCanvas.Shared.ComponentModel;

namespace ClipboardCanvas.Sdk.Services
{
    /// <summary>
    /// The main settings service to manage other settings services.
    /// </summary>
    public interface ISettingsService : IPersistable
    {
        /// <summary>
        /// Gets the service which is used to store application-related settings.
        /// </summary>
        IAppSettings AppSettings { get; }

        /// <summary>
        /// Gets the service which is used to store user-related settings.
        /// </summary>
        IUserSettings UserSettings { get; }
    }
}
