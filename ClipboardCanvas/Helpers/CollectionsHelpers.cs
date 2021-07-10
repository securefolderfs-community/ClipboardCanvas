using System.Collections.Generic;
using System.Linq;
using ClipboardCanvas.ViewModels.UserControls;
using ClipboardCanvas.ViewModels.UserControls.Collections;

namespace ClipboardCanvas.Helpers
{
    public static class CollectionsHelpers
    {
        public static void UpdateLastSelectedCollectionSetting(BaseCollectionViewModel collectionViewModel)
        {
            if (collectionViewModel is DefaultCollectionViewModel)
            {
                App.AppSettings.CollectionLocationsSettings.LastSelectedCollection = Constants.Collections.DEFAULT_COLLECTION_TOKEN;
            }
            else
            {
                App.AppSettings.CollectionLocationsSettings.LastSelectedCollection = collectionViewModel.CollectionPath;
            }
        }

        public static void UpdateSavedCollectionsSetting()
        {
            IEnumerable<string> paths = CollectionsControlViewModel.Collections.Select((item) => item.CollectionPath);

            App.AppSettings.CollectionLocationsSettings.SavedCollectionLocations = paths.ToList();
        }
    }
}
