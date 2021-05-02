using System.Collections.Generic;

namespace ClipboardCanvas.ApplicationSettings.Interfaces
{
    public interface ICollectionLocationsSettings
    {
        List<string> SavedCollectionLocations { get; set; }

        string LastSelectedCollection { get; set; }
    }
}
