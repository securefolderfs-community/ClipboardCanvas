using System.IO;
using Windows.Storage;
using System.Collections.Generic;
using ClipboardCanvas.Serialization;

namespace ClipboardCanvas.Services
{
    public class CollectionsSettingsService : BaseJsonSettingsModel, ICollectionsSettingsService
    {
        #region Constructor

        public CollectionsSettingsService()
            : base(Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.LocalSettings.SETTINGS_FOLDERNAME, Constants.LocalSettings.COLLECTION_SETTINGS_FILENAME),
                  isCachingEnabled: true)
        {
        }

        #endregion

        #region ICollectionLocationsSettings

        public IEnumerable<string> SavedCollectionLocations
        {
            get => Get<IEnumerable<string>>(null);
            set => Set<IEnumerable<string>>(value);
        }

        public string LastSelectedCollection
        {
            get => Get<string>(Constants.Collections.DEFAULT_COLLECTION_TOKEN);
            set => Set<string>(value);
        }

        #endregion
    }
}
