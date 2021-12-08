using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;

namespace ClipboardCanvas.Models
{
    [Serializable]
    public sealed class InfiniteCanvasConfigurationModel
    {
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
        public readonly string associatedItemPath;

        public readonly Vector2 locationVector;

        public readonly int canvasTopIndex;

        [JsonConstructor]
        public InfiniteCanvasConfigurationItemModel(string associatedItemPath, Vector2 locationVector, int canvasTopIndex)
        {
            this.associatedItemPath = associatedItemPath;
            this.locationVector = locationVector;
            this.canvasTopIndex = canvasTopIndex;
        }
    }
}
