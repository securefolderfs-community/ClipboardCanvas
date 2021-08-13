using System.Threading.Tasks;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.Models;
using ClipboardCanvas.Models.Configuration;
using ClipboardCanvas.ViewModels.UserControls.Collections;
using ClipboardCanvas.ViewModels.Widgets.Timeline;

namespace ClipboardCanvas.Services.Implementation
{
    public class TimelineService : ITimelineService
    {
        public TimelineSectionItemViewModel AddItemForSection(TimelineSectionViewModel timelineSection, ICollectionModel collectionModel, CollectionItemViewModel collectionItemViewModel)
        {
            return timelineSection.AddItem(collectionModel, collectionItemViewModel);
        }

        public bool RemoveItemFromSection(TimelineSectionViewModel timelineSection, TimelineSectionItemViewModel timelineSectionItem)
        {
            return timelineSection.RemoveItem(timelineSectionItem);
        }

        public TimelineSectionViewModel GetOrCreateTodaySection()
        {
            return TimelineWidgetViewModel.GetOrCreateTodaySection();
        }

        public async Task LoadSectionAsync(TimelineSectionViewModel timelineSection)
        {
            foreach (var item in timelineSection.Items)
            {
                await item.InitializeSectionItemContent();
            }
        }

        public async Task LoadAllSectionsAsync()
        {
            foreach (var item in TimelineWidgetViewModel.Sections)
            {
                await LoadSectionAsync(item);
            }
        }

        public void UnloadSection(TimelineSectionViewModel timelineSection)
        {
            timelineSection.Dispose();
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
            return timelineSection.FindTimelineSectionItem(canvasItem);
        }

        public TimelineConfigurationModel ConstructConfigurationModel()
        {
            return TimelineWidgetViewModel.ConstructConfigurationModel();
        }
    }
}
