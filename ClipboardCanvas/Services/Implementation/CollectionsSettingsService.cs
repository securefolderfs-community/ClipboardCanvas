using System.IO;
using Windows.Storage;
using System.Collections.Generic;

using ClipboardCanvas.Serialization;
using ClipboardCanvas.Models;

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

        public List<CollectionConfigurationModel> SavedCollections
        {
            get => Get<List<CollectionConfigurationModel>>(null);
            set => Set<List<CollectionConfigurationModel>>(value);
        }

        public string LastSelectedCollection
        {
            get => Get<string>(Constants.Collections.DEFAULT_COLLECTION_TOKEN);
            set => Set<string>(value);
        }

        #endregion
    }
}
