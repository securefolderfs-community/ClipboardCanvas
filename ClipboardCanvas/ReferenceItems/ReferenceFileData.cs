using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ClipboardCanvas.ReferenceItems
{
    [Serializable]
    public sealed class ReferenceFileData
    {
        [JsonRequired]
        public readonly string path;

        [JsonConstructor]
        public ReferenceFileData(string path)
        {
            this.path = path;
        }

        public Dictionary<string, object> ToRawData()
        {
            Dictionary<string, object> rawData = new Dictionary<string, object>();

            rawData.Add(nameof(path), path);

            return rawData;
        }
    }
}
