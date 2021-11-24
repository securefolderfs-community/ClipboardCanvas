using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ClipboardCanvas.Models.JsonSettings.Implementation
{
    public sealed class DerivedTypesJsonSettingsSerializer : IJsonSettingsSerializer
    {
        private readonly JsonSerializerSettings _settings;

        public DerivedTypesJsonSettingsSerializer(ISerializationBinder serializationBinder)
        {
            _settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                SerializationBinder = serializationBinder,
                Formatting = Formatting.Indented
            };
        }

        public T DeserializeFromJson<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, _settings);
            }
            catch
            {
                return default;
            }
        }

        public string SerializeToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, _settings);
        }
    }
}
