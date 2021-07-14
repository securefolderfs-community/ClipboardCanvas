using System.Collections.Generic;

namespace ClipboardCanvas.ApplicationSettings.Interfaces
{
    public interface ICollectionsSettings
    {
        IEnumerable<string> SavedCollectionLocations { get; set; }

        string LastSelectedCollection { get; set; }
    }
}
