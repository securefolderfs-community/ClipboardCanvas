using System.Collections.Generic;
using System.Threading.Tasks;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.Models.Configuration;
using ClipboardCanvas.ViewModels.Widgets.Timeline;

namespace ClipboardCanvas.Services.Implementation
{
    public class TimelineService : ITimelineService
    {
        public async Task<SafeWrapper<TimelineSectionItemViewModel>> AddItemForSection(TimelineSectionViewModel timelineSection, ICollectionModel collectionModel, CanvasItem canvasItem)
        {
            if (!await TimelineWidgetViewModel.CheckIfTimelineEnabled())
            {
                return null;
            }

            return await timelineSection.AddItem(collectionModel, canvasItem);
        }

        public bool RemoveItemFromSection(TimelineSectionViewModel timelineSection, TimelineSectionItemViewModel timelineSectionItem)
        {
            return timelineSection?.RemoveItem(timelineSectionItem) ?? false;
        }

        public async Task<TimelineSectionViewModel> GetOrCreateTodaySection()
        {
            if (!await TimelineWidgetViewModel.CheckIfTimelineEnabled())
            {
                return null;
            }

            return TimelineWidgetViewModel.GetOrCreateTodaySection();
        }

        public async Task LoadSectionAsync(TimelineSectionViewModel timelineSection)
        {
            if (!await TimelineWidgetViewModel.CheckIfTimelineEnabled())
            {
                return;
            }

            List<Task> loadItemTasks = new List<Task>();
            foreach (var item in timelineSection.Items)
            {
                loadItemTasks.Add(item.InitializeSectionItemContent());
            }

            await Task.WhenAll(loadItemTasks);
        }

        public async Task LoadAllSectionsAsync()
        {
            if (!await TimelineWidgetViewModel.CheckIfTimelineEnabled())
            {
                return;
            }

            List<Task> loadItemTasks = new List<Task>();
            foreach (var item in TimelineWidgetViewModel.Sections)
            {
                loadItemTasks.Add(LoadSectionAsync(item));
            }

            await Task.WhenAll(loadItemTasks);
        }

        public void UnloadSection(TimelineSectionViewModel timelineSection)
        {
            timelineSection?.Dispose();
        }

        public void UnloadAllSections()
        {
            foreach (var item in TimelineWidgetViewModel.Sections)
            {
                item.Dispose();
            }
        }

        public TimelineSectionItemViewModel FindTimelineSectionItem(TimelineSectionViewModel timelineSection, CanvasItem canvasItem)
        {
            return timelineSection?.FindTimelineSectionItem(canvasItem);
        }

        public (TimelineSectionViewModel section, TimelineSectionItemViewModel sectionItem) FindTimelineSectionItem(CanvasItem canvasItem)
        {
            TimelineSectionViewModel section = null;
            TimelineSectionItemViewModel sectionItem = null;

            foreach (var item in TimelineWidgetViewModel.Sections)
            {
                sectionItem = item.FindTimelineSectionItem(canvasItem);

                if (sectionItem != null)
                {
                    section = item;
                    break;
                }
            }

            return (section, sectionItem);
        }

        public TimelineConfigurationModel ConstructConfigurationModel()
        {
            return TimelineWidgetViewModel.ConstructConfigurationModel();
        }
    }
}
