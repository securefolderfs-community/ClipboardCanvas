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
                App.AppSettings.CollectionsSettings.LastSelectedCollection = Constants.Collections.DEFAULT_COLLECTION_TOKEN;
            }
            else
            {
                App.AppSettings.CollectionsSettings.LastSelectedCollection = collectionViewModel.CollectionPath;
            }
        }

        public static void UpdateSavedCollectionsSetting()
        {
            IEnumerable<string> paths = CollectionsControlViewModel.Collections.Select((item) => item is DefaultCollectionViewModel ? Constants.Collections.DEFAULT_COLLECTION_TOKEN : item.CollectionPath);

            App.AppSettings.CollectionsSettings.SavedCollectionLocations = paths.ToList();
        }
    }
}
