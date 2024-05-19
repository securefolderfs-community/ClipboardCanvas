using ClipboardCanvas.Sdk.AppModels;
using ClipboardCanvas.Sdk.AppModels.Database;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services.Settings;
using OwlCore.Storage;

namespace ClipboardCanvas.UI.ServiceImplementation.Settings
{
    /// <inheritdoc cref="IAppSettings"/>
    public sealed class AppSettings : SettingsModel, IAppSettings
    {
        /// <inheritdoc/>
        protected override IDatabaseModel<string> SettingsDatabase { get; }

        public AppSettings(IModifiableFolder settingsFolder)
        {
            SettingsDatabase = new SingleFileDatabaseModel(Constants.FileNames.APPLICATION_SETTINGS_FILENAME, settingsFolder, DoubleSerializedStreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public string? ApplicationTheme
        {
            get => GetSetting<string?>();
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public string? LastVersion
        {
            get => GetSetting<string?>();
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public string? AppLanguage
        {
            get => GetSetting<string?>();
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public string? LastLocationId
        {
            get => GetSetting<string?>();
            set => SetSetting(value);
        }
    }
}
