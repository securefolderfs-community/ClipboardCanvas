using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using System.Linq;

using ClipboardCanvas.Models.Configuration;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.Services;
using ClipboardCanvas.ViewModels.UserControls.Collections;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.ViewModels.Widgets.Timeline
{
    public class TimelineWidgetViewModel : ObservableObject
    {
        #region Members

        public static CancellationTokenSource LoadCancellationToken = new CancellationTokenSource();

        #endregion

        #region Properties

        public static ObservableCollection<TimelineSectionViewModel> Sections { get; private set; } = new ObservableCollection<TimelineSectionViewModel>();
        
        public static bool IsInitialized { get; private set; }

        #endregion

        #region Constructor

        public TimelineWidgetViewModel()
        {
        }

        #endregion

        #region Helpers

        public static TimelineSectionViewModel GetOrCreateTodaySection()
        {
            TimelineSectionViewModel todaySection = TimelineWidgetViewModel.Sections.FirstOrDefault((item) => item.SectionDate == DateTime.Today);

            if (todaySection == null)
            {
                todaySection = TimelineWidgetViewModel.AddSection(DateTime.Today);
            }

            return todaySection;
        }

        public static TimelineSectionViewModel AddSection(DateTime sectionDate, bool suppressSettingsUpdate = false)
        {
            var item = new TimelineSectionViewModel(sectionDate);
            AddSectionMode(item, true, suppressSettingsUpdate);

            return item;
        }

        public static TimelineSectionViewModel AddSectionBack(DateTime sectionDate, bool suppressSettingsUpdate = false)
        {
            var item = new TimelineSectionViewModel(sectionDate);
            AddSectionMode(item, false, suppressSettingsUpdate);
           
            return item;
        }

        private static void AddSectionMode(TimelineSectionViewModel timelineSection, bool front, bool suppressSettingsUpdate = false)
        {
            if (front)
            {
                Sections.AddFront(timelineSection);
            }
            else
            {
                Sections.Add(timelineSection);
            }

            if (Sections.Count > Constants.UI.Timeline.MAX_SECTIONS)
            {
                Sections.RemoveBack();
            }    

            if (!suppressSettingsUpdate)
            {
                SettingsSerializationHelpers.UpdateUserTimelineSetting();
            }
        }

        public static async Task ReloadAllSections()
        {
            if (!await CheckIfTimelineEnabled(false))
            {
                return;
            }

            IsInitialized = true;
            ITimelineSettingsService timelineSettingsService = Ioc.Default.GetService<ITimelineSettingsService>();
            TimelineConfigurationModel configurationModel = timelineSettingsService.UserTimeline;

            if (configurationModel != null)
            {
                foreach (var configurationSection in configurationModel.sections)
                {
                    var section = AddSectionBack(configurationSection.sectionDateTime, true);

                    foreach (var configurationSectionItem in configurationSection.items)
                    {
                        BaseCollectionViewModel baseCollectionViewModel = CollectionsWidgetViewModel.FindCollection(configurationSectionItem.collectionConfigurationModel);
                        SafeWrapper<IStorageItem> item = await StorageHelpers.ToStorageItemWithError<IStorageItem>(configurationSectionItem.collectionItemPath);

                        if (baseCollectionViewModel != null && item)
                        {
                            CanvasItem canvasItem = new CanvasItem(item.Result);
                            section.AddItemBack(baseCollectionViewModel, canvasItem, true);
                        }
                    }
                }

                RemoveEmptySections();
            }

            // Add "Today" section
            GetOrCreateTodaySection();
        }

        public static TimelineConfigurationModel ConstructConfigurationModel()
        {
            TimelineConfigurationModel configurationModel = new TimelineConfigurationModel();
            
            foreach (var item in Sections)
            {
                configurationModel.sections.Add(item.ConstructConfigurationModel());
            }

            return configurationModel;
        }

        private static void RemoveEmptySections()
        {
            for (int i = 0; i < Sections.Count; i++)
            {
                if (Sections[i].Items.IsEmpty())
                {
                    Sections.RemoveAt(i);
                }
            }
        }

        public static async Task<bool> CheckIfTimelineEnabled(bool initializeIfNecessary = true)
        {
            IUserSettingsService userSettingsService = Ioc.Default.GetService<IUserSettingsService>();

            // Don't load if we don't use it
            if (!userSettingsService.ShowTimelineOnHomepage)
            {
                return false;
            }
            else if (!IsInitialized && initializeIfNecessary)
            {
                await ReloadAllSections();
            }

            return true;
        }

        #endregion
    }
}
