using System.Collections.Generic;

namespace ClipboardCanvas.Services
{
    public interface ICollectionsSettingsService
    {
        IEnumerable<string> SavedCollectionLocations { get; set; }

        string LastSelectedCollection { get; set; }
    }
}
