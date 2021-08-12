using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

using ClipboardCanvas.Services;
using ClipboardCanvas.ViewModels.UserControls.Collections;
using ClipboardCanvas.Models;

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

            List<CollectionConfigurationModel> configurations = CollectionsControlViewModel.Collections.Select((item) => item.ConstructCollectionConfigurationModel()).ToList();
            collectionsSettings.SavedCollections = configurations;
        }
    }
}
