using System.Collections.Generic;
using ClipboardCanvas.ViewModels.UserControls;

namespace ClipboardCanvas.Helpers
{
    public static class CollectionsHelpers
    {
        public static void UpdateLastSelectedCollectionSetting(CollectionsContainerViewModel container)
        {
            if (container.isDefault)
            {
                App.AppSettings.CollectionLocationsSettings.LastSelectedCollection = Constants.Collections.DEFAULT_COLLECTION_TOKEN;
            }
            else
            {
                App.AppSettings.CollectionLocationsSettings.LastSelectedCollection = container.DangerousGetCollectionFolder()?.Path;
            }
        }

        public static void UpdateSavedCollectionLocationsSetting()
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
