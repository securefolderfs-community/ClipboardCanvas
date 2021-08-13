using System.Threading.Tasks;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.Models;
using ClipboardCanvas.Models.Configuration;
using ClipboardCanvas.ViewModels.UserControls.Collections;
using ClipboardCanvas.ViewModels.Widgets.Timeline;

namespace ClipboardCanvas.Services
{
    public interface ITimelineService
    {
        TimelineSectionItemViewModel AddItemForSection(TimelineSectionViewModel timelineSection, ICollectionModel collectionModel, CollectionItemViewModel collectionItemViewModel);

        bool RemoveItemFromSection(TimelineSectionViewModel timelineSection, TimelineSectionItemViewModel timelineSectionItem);

        TimelineSectionViewModel GetOrCreateTodaySection();

        Task LoadSectionAsync(TimelineSectionViewModel timelineSection);

        Task LoadAllSectionsAsync();

        void UnloadSection(TimelineSectionViewModel timelineSection);

        void UnloadAllSections();

        TimelineSectionItemViewModel FindTimelineSectionItem(TimelineSectionViewModel timelineSection, CanvasItem canvasItem);

        TimelineConfigurationModel ConstructConfigurationModel();
    }
}
