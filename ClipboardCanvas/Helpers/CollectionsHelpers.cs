using System.Linq;
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
            App.AppSettings.CollectionLocationsSettings.SavedCollectionLocations = CollectionsControlViewModel.Items.Where((item) => !item.isDefault).Select((item) => item.CollectionFolderPath).ToList();
        }
    }
}
