using System.Collections.Generic;
using ClipboardCanvas.Models;

namespace ClipboardCanvas.Services
{
    public interface ICollectionsSettingsService
    {
        IEnumerable<CollectionConfigurationModel> SavedCollections { get; set; }

        string LastSelectedCollection { get; set; }
    }
}
