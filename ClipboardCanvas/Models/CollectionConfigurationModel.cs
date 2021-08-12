using Newtonsoft.Json;
using System;

namespace ClipboardCanvas.Models
{
    [Serializable]
    public sealed class CollectionConfigurationModel
    {
        public readonly string collectionPath;

        public readonly bool usesCustomIcon;

        public readonly string iconFileName;

        public CollectionConfigurationModel(string collectionPath)
            : this(collectionPath, false, null)
        {
        }

        [JsonConstructor]
        public CollectionConfigurationModel(string collectionPath, bool usesCustomIcon, string iconFileName)
        {
            this.collectionPath = collectionPath;
            this.usesCustomIcon = usesCustomIcon;
            this.iconFileName = iconFileName;
        }
    }
}
