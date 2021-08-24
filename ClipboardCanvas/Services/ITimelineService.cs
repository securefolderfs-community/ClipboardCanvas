using System.Threading.Tasks;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.Models;
using ClipboardCanvas.Models.Configuration;
using ClipboardCanvas.ViewModels.Widgets.Timeline;

namespace ClipboardCanvas.Services
{
    public interface ITimelineService
    {
        Task<TimelineSectionItemViewModel> AddItemForSection(TimelineSectionViewModel timelineSection, ICollectionModel collectionModel, CanvasItem canvasItem);

        bool RemoveItemFromSection(TimelineSectionViewModel timelineSection, TimelineSectionItemViewModel timelineSectionItem);

        Task<TimelineSectionViewModel> GetOrCreateTodaySection();

        Task LoadSectionAsync(TimelineSectionViewModel timelineSection);

        Task LoadAllSectionsAsync();

        void UnloadSection(TimelineSectionViewModel timelineSection);

        void UnloadAllSections();

        TimelineSectionItemViewModel FindTimelineSectionItem(TimelineSectionViewModel timelineSection, CanvasItem canvasItem);

        (TimelineSectionViewModel section, TimelineSectionItemViewModel sectionItem) FindTimelineSectionItem(CanvasItem canvasItem);

        TimelineConfigurationModel ConstructConfigurationModel();
    }
}
