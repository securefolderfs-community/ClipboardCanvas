using ClipboardCanvas.Models.Configuration;

namespace ClipboardCanvas.Services
{
    public interface ITimelineSettingsService
    {
        TimelineConfigurationModel UserTimeline { get; set; }
    }
}
