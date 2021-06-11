using ClipboardCanvas.ApplicationSettings.Interfaces;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;

namespace ClipboardCanvas.ApplicationSettings
{
    public class CollectionLocationsSettingsModel : BaseJsonSettingsModel, ICollectionLocationsSettings
    {
        #region Constructor

        public CollectionLocationsSettingsModel()
            : base(Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.LocalSettings.SETTINGS_FOLDERNAME, Constants.LocalSettings.COLLECTION_LOCATIONS_FILENAME),
                  isCachingEnabled: true)
        {
        }

        #endregion

        #region ICollectionLocationsSettings

        public List<string> SavedCollectionLocations
        {
            get => Get<List<string>>(null);
            set => Set<List<string>>(value);
        }

        public string LastSelectedCollection
        {
            get => Get<string>(Constants.Collections.DEFAULT_COLLECTION_TOKEN);
            set => Set<string>(value);
        }

        #endregion
    }
}
