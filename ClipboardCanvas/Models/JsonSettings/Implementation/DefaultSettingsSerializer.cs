using ClipboardCanvas.UnsafeNative;

namespace ClipboardCanvas.Models.JsonSettings.Implementation
{
    public class DefaultSettingsSerializer : ISettingsSerializer
    {
        private readonly string _filePath;

        public DefaultSettingsSerializer(string filePath)
        {
            this._filePath = filePath;
        }

        public string ReadFromFile()
        {
            return UnsafeNativeHelpers.ReadStringFromFile(_filePath);
        }

        public bool WriteToFile(string json)
        {
            return UnsafeNativeHelpers.WriteStringToFile(_filePath, json);
        }
    }
}
