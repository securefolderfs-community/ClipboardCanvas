using ClipboardCanvas.ApplicationSettings.Interfaces;

namespace ClipboardCanvas.ApplicationSettings
{
    public class SettingsInstance
    {
        public IUserSettings UserSettings;

        public ICollectionLocationsSettings CollectionLocationsSettings;

        public SettingsInstance()
        {
            UserSettings = new UserSettingsModel();
            CollectionLocationsSettings = new CollectionLocationsSettingsModel();
        }
    }
}
