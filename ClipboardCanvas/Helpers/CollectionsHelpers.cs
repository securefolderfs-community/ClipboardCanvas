using System.Collections.Generic;
using System.Linq;
using ClipboardCanvas.Services;
using ClipboardCanvas.ViewModels.UserControls;
using ClipboardCanvas.ViewModels.UserControls.Collections;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace ClipboardCanvas.Helpers
{
    public static class CollectionsHelpers
    {
        public static void UpdateLastSelectedCollectionSetting(BaseCollectionViewModel collectionViewModel)
        {
            ICollectionsSettingsService collectionsSettings = Ioc.Default.GetService<ICollectionsSettingsService>();

            if (collectionViewModel is DefaultCollectionViewModel)
            {
                collectionsSettings.LastSelectedCollection = Constants.Collections.DEFAULT_COLLECTION_TOKEN;
            }
            else
            {
                collectionsSettings.LastSelectedCollection = collectionViewModel.CollectionPath;
            }
        }

        public static void UpdateSavedCollectionsSetting()
        {
            ICollectionsSettingsService collectionsSettings = Ioc.Default.GetService<ICollectionsSettingsService>();
            IEnumerable<string> paths = CollectionsControlViewModel.Collections.Select((item) => item is DefaultCollectionViewModel ? Constants.Collections.DEFAULT_COLLECTION_TOKEN : item.CollectionPath);

            collectionsSettings.SavedCollectionLocations = paths.ToList();
        }
    }
}
