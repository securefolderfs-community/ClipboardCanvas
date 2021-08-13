using System.IO;
using Windows.Storage;
using ClipboardCanvas.Serialization;

namespace ClipboardCanvas.Services
{
    public class UserSettingsService : BaseJsonSettingsModel, IUserSettingsService
    {
        #region Constructor

        public UserSettingsService() 
            : base(Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.LocalSettings.SETTINGS_FOLDERNAME, Constants.LocalSettings.USER_SETTINGS_FILENAME),
                  isCachingEnabled: true)
        {
        }

        #endregion

        #region IUserSettings

        public bool ShowTimelineOnHomepage
        {
            get => Get<bool>(true);
            set => Set<bool>(value);
        }

        public bool OpenNewCanvasOnPaste
        {
            get => Get<bool>(false);
            set => Set<bool>(value);
        }

        public bool AlwaysPasteFilesAsReference
        {
            get => Get<bool>(true);
            set => Set<bool>(value);
        }

        public bool PrioritizeMarkdownOverText
        {
            get => Get<bool>(false);
            set => Set<bool>(value);
        }

        public bool ShowDeleteConfirmationDialog
        {
            get => Get<bool>(true);
            set => Set<bool>(value);
        }

        public bool UseInfiniteCanvasAsDefault
        {
            get => Get<bool>(true);
            set => Set<bool>(value);
        }

        #endregion
    }
}
