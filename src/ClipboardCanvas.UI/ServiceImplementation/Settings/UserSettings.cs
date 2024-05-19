using ClipboardCanvas.Sdk.AppModels;
using ClipboardCanvas.Sdk.AppModels.Database;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services.Settings;
using OwlCore.Storage;

namespace ClipboardCanvas.UI.ServiceImplementation.Settings
{
    /// <inheritdoc cref="IUserSettings"/>
    public sealed class UserSettings : SettingsModel, IUserSettings
    {
        /// <inheritdoc/>
        protected override IDatabaseModel<string> SettingsDatabase { get; }

        public UserSettings(IModifiableFolder settingsFolder)
        {
            SettingsDatabase = new SingleFileDatabaseModel(Constants.FileNames.USER_SETTINGS_FILENAME, settingsFolder, DoubleSerializedStreamSerializer.Instance);
        }

        public bool UninterruptedResume
        {
            get => GetSetting(static () => false);
            set => SetSetting(value);
        }
    }
}
