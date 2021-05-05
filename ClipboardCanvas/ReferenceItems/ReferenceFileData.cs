using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.ReferenceItems
{
    [Serializable]
    public class ReferenceFileData
    {
        [JsonRequired]
        public readonly string path;

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
