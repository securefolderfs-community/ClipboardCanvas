using System.IO;
using Windows.Storage;

using ClipboardCanvas.Models.Configuration;
using ClipboardCanvas.Serialization;

namespace ClipboardCanvas.Services.Implementation
{
    public class TimelineSettingsService : BaseJsonSettingsModel, ITimelineSettingsService
    {
        public TimelineSettingsService()
            : base(Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.LocalSettings.SETTINGS_FOLDERNAME, Constants.LocalSettings.TIMELINE_SETTINGS_FILENAME),
                  isCachingEnabled: true)
        {
        }

        public TimelineConfigurationModel UserTimeline
        {
            get => Get<TimelineConfigurationModel>(null);
            set => Set<TimelineConfigurationModel>(value);
        }
    }
}
