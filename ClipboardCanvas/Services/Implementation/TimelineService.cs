using System.Threading.Tasks;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.Models;
using ClipboardCanvas.Models.Configuration;
using ClipboardCanvas.ViewModels.Widgets.Timeline;

namespace ClipboardCanvas.Services.Implementation
{
    public class TimelineService : ITimelineService
    {
        public async Task<TimelineSectionItemViewModel> AddItemForSection(TimelineSectionViewModel timelineSection, ICollectionModel collectionModel, CanvasItem canvasItem)
        {
            if (!await TimelineWidgetViewModel.CheckIfTimelineEnabled())
            {
                return null;
            }

            return timelineSection.AddItem(collectionModel, canvasItem);
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

            foreach (var item in timelineSection.Items)
            {
                await item.InitializeSectionItemContent();
            }
        }

        public async Task LoadAllSectionsAsync()
        {
            if (!await TimelineWidgetViewModel.CheckIfTimelineEnabled())
            {
                return;
            }

            foreach (var item in TimelineWidgetViewModel.Sections)
            {
                await LoadSectionAsync(item);
            }
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

        public TimelineConfigurationModel ConstructConfigurationModel()
        {
            return TimelineWidgetViewModel.ConstructConfigurationModel();
        }
    }
}
