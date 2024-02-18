using ClipboardCanvas.Sdk.AppModels;
using ClipboardCanvas.Sdk.AppModels.Database;
using ClipboardCanvas.Sdk.DataModels;
using ClipboardCanvas.Sdk.Models;
using ClipboardCanvas.Sdk.Services;
using OwlCore.Storage;
using System.Collections.Generic;

namespace ClipboardCanvas.UI.ServiceImplementation
{
    /// <inheritdoc cref="ICollectionPersistenceService"/>
    public sealed class CollectionPersistenceService : SettingsModel, ICollectionPersistenceService
    {
        /// <inheritdoc/>
        protected override IDatabaseModel<string> SettingsDatabase { get; }

        public CollectionPersistenceService(IModifiableFolder settingsFolder)
        {
            SettingsDatabase = new SingleFileDatabaseModel(Constants.FileNames.SAVED_COLLECTIONS_FILENAME, settingsFolder, DoubleSerializedStreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public IList<CollectionDataModel>? SavedCollections { get; set; }
    }
}
