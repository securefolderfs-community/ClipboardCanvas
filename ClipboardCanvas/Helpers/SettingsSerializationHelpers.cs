using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

using ClipboardCanvas.Services;
using ClipboardCanvas.ViewModels.UserControls.Collections;
using ClipboardCanvas.Models;

namespace ClipboardCanvas.Helpers
{
    public class SettingsSerializationHelpers
    {
        public static void UpdateUserTimelineSetting()
        {
            ITimelineService timelineService = Ioc.Default.GetService<ITimelineService>();
            ITimelineSettingsService timelineSettingsService = Ioc.Default.GetService<ITimelineSettingsService>();

            var configurationModel = timelineService.ConstructConfigurationModel();
            timelineSettingsService.UserTimeline = configurationModel;
        }

        public static void UpdateLastSelectedCollectionSetting(BaseCollectionViewModel collectionViewModel)
        {
            ICollectionsSettingsService collectionsSettings = Ioc.Default.GetService<ICollectionsSettingsService>();

            var collectionConfiguration = collectionViewModel.ConstructConfigurationModel();
            collectionsSettings.LastSelectedCollection = collectionConfiguration.collectionPath;
        }

        public static void UpdateSavedCollectionsSetting()
        {
            ICollectionsSettingsService collectionsSettings = Ioc.Default.GetService<ICollectionsSettingsService>();

            IEnumerable<CollectionConfigurationModel> configurations = CollectionsWidgetViewModel.Collections.Select((item) => item.ConstructConfigurationModel());
            collectionsSettings.SavedCollections = configurations.ToList();
        }
    }
}
