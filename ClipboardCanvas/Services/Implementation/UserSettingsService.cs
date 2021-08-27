using System.IO;
using Windows.Storage;
using Microsoft.AppCenter.Analytics;

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
            TrackAppCenterAnalytics();
        }

        #endregion

        #region Private Helpers

        private void TrackAppCenterAnalytics()
        {
            Analytics.TrackEvent($"{nameof(PushErrorNotification)} {PushErrorNotification}");
            Analytics.TrackEvent($"{nameof(ShowTimelineOnHomepage)} {ShowTimelineOnHomepage}");
            Analytics.TrackEvent($"{nameof(DeletePermanentlyAsDefault)} {DeletePermanentlyAsDefault}");
            Analytics.TrackEvent($"{nameof(OpenNewCanvasOnPaste)} {OpenNewCanvasOnPaste}");
            Analytics.TrackEvent($"{nameof(AlwaysPasteFilesAsReference)} {AlwaysPasteFilesAsReference}");
            Analytics.TrackEvent($"{nameof(PrioritizeMarkdownOverText)} {PrioritizeMarkdownOverText}");
            Analytics.TrackEvent($"{nameof(ShowDeleteConfirmationDialog)} {ShowDeleteConfirmationDialog}");
            Analytics.TrackEvent($"{nameof(UseInfiniteCanvasAsDefault)} {UseInfiniteCanvasAsDefault}");
        }

        #endregion

        #region IUserSettings

        public bool PushErrorNotification
        {
            get => Get<bool>(true);
            set => Set<bool>(value);
        }

        public bool ShowTimelineOnHomepage
        {
            get => Get<bool>(true);
            set => Set<bool>(value);
        }

        public bool DeletePermanentlyAsDefault
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
            get => App.IsInRestrictedAccessMode ? false : Get<bool>(true);
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
