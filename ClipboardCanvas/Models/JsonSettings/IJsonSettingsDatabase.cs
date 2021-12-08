using System;

namespace ClipboardCanvas.Models.JsonSettings
{
    public interface IJsonSettingsDatabase
    {
        Type DataBaseObjectType { get; }

        TValue GetValue<TValue>(string key, TValue defaultValue = default);

        bool AddKey(string key, object value);

        bool RemoveKey(string key);

        bool UpdateKey(string key, object newValue);

        bool FlushSettings();

        bool ImportSettings(object import);

        object ExportSettings();
    }
}
