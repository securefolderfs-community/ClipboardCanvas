using ClipboardCanvas.Helpers;
using ClipboardCanvas.UnsafeNative;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace ClipboardCanvas.ApplicationSettings
{
    public abstract class BaseJsonSettingsModel
    {
        #region Protected Members

        protected Dictionary<string, object> serializableSettings;

        protected string settingsPath;

        #endregion

        #region Constructor

        public BaseJsonSettingsModel(string settingsPath)
        {
            this.settingsPath = settingsPath;

            Initialize();
        }

        #endregion

        #region Helpers

        protected virtual async void Initialize()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync(settingsPath.Replace(ApplicationData.Current.LocalFolder.Path, string.Empty), CreationCollisionOption.OpenIfExists);
        }

        public virtual object ExportSettings()
        {
            return serializableSettings;
        }

        public virtual void ImportSettings(object import)
        {
            try
            {
                serializableSettings = (Dictionary<string, object>)import;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Debugger.Break();
            }
        }

        #endregion

        #region Get, Set

        protected virtual TValue Get<TValue>(TValue defaultValue, [CallerMemberName] string propertyName = "")
        {
            try
            {
                string settingsData = UnsafeNativeHelpers.ReadStringFromFile(settingsPath);

                if (!string.IsNullOrEmpty(settingsData))
                {
                    serializableSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(settingsData);
                }

                if (serializableSettings == null)
                    serializableSettings = new Dictionary<string, object>();

                if (!serializableSettings.ContainsKey(propertyName))
                {
                    serializableSettings.Add(propertyName, defaultValue);

                    // Serialize
                    UnsafeNativeHelpers.WriteStringToFile(settingsPath, JsonConvert.SerializeObject(serializableSettings, Formatting.Indented));
                }

                object valueObject = serializableSettings[propertyName];

                if (valueObject == null)
                {
                    return defaultValue;
                }

                if (valueObject is JToken jtoken)
                {
                    return jtoken.ToObject<TValue>();
                }

                return (TValue)valueObject;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Debugger.Break();
                return default(TValue);
            }
        }

        protected virtual bool Set<TValue>(TValue value, [CallerMemberName] string propertyName = "")
        {
            try
            {
                if (!serializableSettings.ContainsKey(propertyName))
                {
                    serializableSettings.Add(propertyName, value);
                }
                else
                {
                    serializableSettings[propertyName] = value;
                }

                // Serialize
                UnsafeNativeHelpers.WriteStringToFile(settingsPath, JsonConvert.SerializeObject(serializableSettings, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Debugger.Break();
                return false;
            }
            return true;
        }

        #endregion
    }
}
