using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using ClipboardCanvas.ApplicationSettings.Interfaces;

namespace ClipboardCanvas.ApplicationSettings
{
    public class CollectionsSettingsModel : BaseJsonSettingsModel, ICollectionsSettings
    {
        #region Constructor

        public CollectionsSettingsModel()
            : base(Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.LocalSettings.SETTINGS_FOLDERNAME, Constants.LocalSettings.COLLECTION_LOCATIONS_FILENAME),
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
