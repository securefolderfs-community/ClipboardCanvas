using ClipboardCanvas.ApplicationSettings.Interfaces;

namespace ClipboardCanvas.ApplicationSettings
{
    public class SettingsInstance
    {
        public IUserSettings UserSettings { get; private set; }

        public ICollectionsSettings CollectionsSettings { get; private set; }

        public ICanvasSettings CanvasSettings { get; private set; }

        public IApplicationSettings ApplicationSettings { get; private set; }

        public SettingsInstance()
        {
            UserSettings = new UserSettingsModel();
            CollectionsSettings = new CollectionsSettingsModel();
            CanvasSettings = new CanvasSettingsModel();
            ApplicationSettings = new ApplicationSettingsModel();
        }
    }
}
