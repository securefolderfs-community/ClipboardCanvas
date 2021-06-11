using ClipboardCanvas.ApplicationSettings.Interfaces;
using System.IO;
using Windows.Storage;

namespace ClipboardCanvas.ApplicationSettings
{
    public class UserSettingsModel : BaseJsonSettingsModel, IUserSettings
    {
        #region Constructor

        public UserSettingsModel() 
            : base(Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.LocalSettings.SETTINGS_FOLDERNAME, Constants.LocalSettings.USER_SETTINGS_FILENAME),
                  isCachingEnabled: true)
        {
        }

        #endregion

        #region IUserSettings

        public bool EnableDetailedLogging
        {
            get => Get<bool>(false);
            set => Set<bool>(value);
        }

        public bool OpenNewCanvasOnPaste
        {
            get => Get<bool>(false);
            set => Set<bool>(value);
        }

        public bool AlwaysPasteFilesAsReference
        {
            get => Get<bool>(false);
            set => Set<bool>(value);
        }

        public bool PrioritizeMarkdownOverText
        {
            get => Get<bool>(false);
            set => Set<bool>(value);
        }

        #endregion
    }
}
