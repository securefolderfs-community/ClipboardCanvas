using ClipboardCanvas.Sdk.DataModels;
using ClipboardCanvas.Shared.ComponentModel;
using System.Collections.Generic;

namespace ClipboardCanvas.Sdk.Services
{
    /// <summary>
    /// Represents a service to manage collection-related data.
    /// </summary>
    public interface ICollectionPersistenceService : IPersistable
    {
        /// <summary>
        /// Gets or sets the list of saved collections.
        /// </summary>
        IList<CollectionDataModel>? SavedCollections { get; set; }
    }
}
