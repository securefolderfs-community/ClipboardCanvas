using Microsoft.Toolkit.Uwp;
using System;
using System.Globalization;

namespace ClipboardCanvas.DataModels
{
    [Serializable]
    public sealed class AppLanguageModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public AppLanguageModel(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var info = new CultureInfo(id);
                Id = info.Name;
                Name = info.NativeName;
            }
            else
            {
                Id = string.Empty;
                var systemDefaultLanguageOptionStr = "SettingsGeneralPageDefaultLanguage".GetLocalized();
                Name = string.IsNullOrEmpty(systemDefaultLanguageOptionStr) ? "System Default" : systemDefaultLanguageOptionStr;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
