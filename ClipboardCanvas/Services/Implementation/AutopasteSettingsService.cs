using ClipboardCanvas.Models.JsonSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.Services.Implementation
{
    public sealed class AutopasteSettingsService : BaseJsonSettingsModel, IAutopasteSettingsService
    {
        public AutopasteSettingsService()
            : base (Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.LocalSettings.SETTINGS_FOLDERNAME, Constants.LocalSettings.AUTOPASTE_SETTINGS_FILENAME),
                  isCachingEnabled: true)
        {
        }

        public string AutopastePath
        {
            get => Get<string>(null);
            set => Set(value);
        }
    }
}
