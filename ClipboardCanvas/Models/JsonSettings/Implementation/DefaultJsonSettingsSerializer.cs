using Newtonsoft.Json;

namespace ClipboardCanvas.Models.JsonSettings.Implementation
{
    public sealed class DefaultJsonSettingsSerializer : IJsonSettingsSerializer
    {
        public T DeserializeFromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public string SerializeToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
    }
}
