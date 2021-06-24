using System.Collections.Generic;
using ClipboardCanvas.ViewModels.UserControls;

namespace ClipboardCanvas.Helpers
{
    public static class CollectionsHelpers
    {
        public static void UpdateLastSelectedCollectionSetting(CollectionViewModel collectionViewModel)
        {
            if (collectionViewModel.isDefault)
            {
                App.AppSettings.CollectionLocationsSettings.LastSelectedCollection = Constants.Collections.DEFAULT_COLLECTION_TOKEN;
            }
            else
            {
                App.AppSettings.CollectionLocationsSettings.LastSelectedCollection = collectionViewModel.DangerousGetCollectionFolder()?.Path;
            }
        }

        public static void UpdateSavedCollectionsSetting()
        {
            List<string> paths = new List<string>();
            foreach (var item in CollectionsControlViewModel.Items)
            {
                if (!item.isDefault)
                {
                    paths.Add(item.CollectionFolderPath);
                }
            }

            App.AppSettings.CollectionLocationsSettings.SavedCollectionLocations = paths;
        }
    }
}
