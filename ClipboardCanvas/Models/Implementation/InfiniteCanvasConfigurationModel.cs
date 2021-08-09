using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;

namespace ClipboardCanvas.Models
{
    [Serializable]
    public sealed class InfiniteCanvasConfigurationModel
    {
        [JsonRequired]
        public readonly List<InfiniteCanvasConfigurationItemModel> elements;

        [JsonConstructor]
        public InfiniteCanvasConfigurationModel()
        {
            this.elements = new List<InfiniteCanvasConfigurationItemModel>();
        }
    }

    [Serializable]
    public sealed class InfiniteCanvasConfigurationItemModel
    {
        [JsonRequired]
        public readonly string associatedItemPath;

        [JsonRequired]
        public readonly Vector2 locationVector;

        [JsonConstructor]
        public InfiniteCanvasConfigurationItemModel(string associatedItemPath, Vector2 locationVector)
        {
            this.associatedItemPath = associatedItemPath;
            this.locationVector = locationVector;
        }
    }
}
