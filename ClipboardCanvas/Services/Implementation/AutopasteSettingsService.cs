using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;

using ClipboardCanvas.Models.JsonSettings;
using ClipboardCanvas.Models.JsonSettings.Implementation;
using ClipboardCanvas.ViewModels.UserControls.Autopaste.Rules;

namespace ClipboardCanvas.Services.Implementation
{
    public sealed class AutopasteSettingsService : BaseJsonSettingsModel, IAutopasteSettingsService
    {
        public AutopasteSettingsService()
            : base (Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.LocalSettings.SETTINGS_FOLDERNAME, Constants.LocalSettings.AUTOPASTE_SETTINGS_FILENAME),
                  isCachingEnabled: true,
                  jsonSettingsSerializer: new DerivedTypesJsonSettingsSerializer(new AutopasteSerializationBinder(typeof(Dictionary<string, object>))))
        {
        }

        public string AutopastePath
        {
            get => Get<string>(null);
            set => Set(value);
        }

        public List<BaseAutopasteRuleViewModel> SavedRules
        {
            get => Get<List<BaseAutopasteRuleViewModel>>(null);
            set => Set(value);
        }
    }

    public sealed class AutopasteSerializationBinder : ISerializationBinder
    {
        private readonly List<Type> _knownTypes;

        public AutopasteSerializationBinder(Type dataBaseObjectType)
        {
            _knownTypes = new List<Type>()
            {
                dataBaseObjectType,
                typeof(BaseAutopasteRuleViewModel),
                typeof(FileSizeRuleViewModel),
                typeof(TypeFilterRuleViewModel),
                typeof(List<BaseAutopasteRuleViewModel>)
            };
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            return _knownTypes.SingleOrDefault(t => t.Name == typeName);
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }
    }
}
