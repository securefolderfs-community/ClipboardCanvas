using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ClipboardCanvas.Models.Configuration
{
    [Serializable]
    public sealed class TimelineConfigurationModel
    {
        public readonly List<TimelineSectionConfigurationModel> sections;

        [JsonConstructor]
        public TimelineConfigurationModel()
        {
            this.sections = new List<TimelineSectionConfigurationModel>();
        }
    }

    public sealed class TimelineSectionConfigurationModel
    {
        public readonly DateTime sectionDateTime;

        public readonly List<TimelineSectionItemConfigurationModel> items;

        [JsonConstructor]
        public TimelineSectionConfigurationModel(DateTime sectionDateTime)
        {
            this.sectionDateTime = sectionDateTime;
            this.items = new List<TimelineSectionItemConfigurationModel>();
        }
    }

    public sealed class TimelineSectionItemConfigurationModel
    {
        public readonly string collectionItemPath;

        public readonly CollectionConfigurationModel collectionConfigurationModel;

        [JsonConstructor]
        public TimelineSectionItemConfigurationModel(string collectionItemPath, CollectionConfigurationModel collectionConfigurationModel)
        {
            this.collectionItemPath = collectionItemPath;
            this.collectionConfigurationModel = collectionConfigurationModel;
        }
    }
}
