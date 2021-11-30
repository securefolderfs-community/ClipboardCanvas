using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using System.Linq;
using System.Collections.Generic;

using ClipboardCanvas.Models.Configuration;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.Services;
using ClipboardCanvas.ViewModels.UserControls.Collections;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.EventArguments;

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

        #region Event Handlers

        private static void Item_OnRemoveSectionRequestedEvent(object sender, RemoveSectionRequestedEventArgs e)
        {
            if (!IsTodaySection(e.sectionViewModel))
            {
                RemoveSection(e.sectionViewModel);
            }
        }

        #endregion

        #region Helpers

        private static bool IsTodaySection(TimelineSectionViewModel section)
        {
            if (section == null)
            {
                return false;
            }

            DateTime now = DateTime.Now;

            return section.SectionDate.DayOfYear == now.DayOfYear && section.SectionDate.Year == now.Year;
        }

        public static TimelineSectionViewModel GetOrCreateTodaySection()
        {
            TimelineSectionViewModel todaySection = TimelineWidgetViewModel.Sections.FirstOrDefault((item) => IsTodaySection(item));

            if (todaySection == null)
            {
                todaySection = TimelineWidgetViewModel.AddSection(DateTime.Today);
            }

            return todaySection;
        }

        public static void RemoveSection(TimelineSectionViewModel timelineSection, bool suppressSettingsUpdate = false)
        {
            timelineSection.OnRemoveSectionRequestedEvent -= Item_OnRemoveSectionRequestedEvent;
            timelineSection.Clean();
            timelineSection.Dispose();

            Sections.Remove(timelineSection);

            if (!suppressSettingsUpdate)
            {
                SettingsSerializationHelpers.UpdateUserTimelineSetting();
            }
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

            timelineSection.OnRemoveSectionRequestedEvent += Item_OnRemoveSectionRequestedEvent;

            if (Sections.Count > Constants.UI.Timeline.MAX_SECTIONS)
            {
                RemoveSection(Sections.Last(), suppressSettingsUpdate);
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
                List<TimelineSectionConfigurationModel> sortedSections = configurationModel.sections.OrderBy((x) => x.sectionDateTime).ToList();

                foreach (var configurationSection in sortedSections)
                {
                    var section = AddSection(configurationSection.sectionDateTime, true);

                    foreach (var configurationSectionItem in configurationSection.items)
                    {
                        SafeWrapper<IStorageItem> item = await StorageHelpers.ToStorageItemWithError<IStorageItem>(configurationSectionItem?.collectionItemPath);

                        if (item)
                        {
                            BaseCollectionViewModel baseCollectionViewModel = CollectionsWidgetViewModel.FindCollection(configurationSectionItem.collectionConfigurationModel);
                            if (baseCollectionViewModel != null)
                            {
                                CanvasItem canvasItem = new CanvasItem(item.Result);
                                await section.AddItemBack(baseCollectionViewModel, canvasItem, true);
                            }
                        }
                    }
                }
            }

            RemoveEmptySections();
            RemoveDuplicateSections();

            // Add "Today" section
            GetOrCreateTodaySection();
        }

        private static void RemoveEmptySections()
        {
            for (int i = 0; i < Sections.Count; i++)
            {
                if (Sections[i].Items.IsEmpty())
                {
                    RemoveSection(Sections[i]);
                    i--;
                }
            }
        }

        private static void RemoveDuplicateSections()
        {
            for (int i = 0; i < Sections.Count; i++)
            {
                // The list is sorted
                for (int j = i + 1; j < Sections.Count; j++)
                {
                    if (Sections[i].SectionDate == Sections[j].SectionDate)
                    {
                        RemoveSection(Sections[i]);
                        j--;
                    }
                }
            }
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
