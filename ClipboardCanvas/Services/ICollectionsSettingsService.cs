using System.Collections.Generic;
using ClipboardCanvas.Models;

namespace ClipboardCanvas.Services
{
    public interface ICollectionsSettingsService
    {
        List<CollectionConfigurationModel> SavedCollections { get; set; }

        string LastSelectedCollection { get; set; }
    }
}
