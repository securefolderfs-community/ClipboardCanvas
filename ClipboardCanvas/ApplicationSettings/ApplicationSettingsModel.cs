using ClipboardCanvas.ApplicationSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.ApplicationSettings
{
    public class ApplicationSettingsModel : BaseJsonSettingsModel, IApplicationSettings
    {
        #region Constructor

        public ApplicationSettingsModel()
            : base (Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.LocalSettings.SETTINGS_FOLDERNAME, Constants.LocalSettings.APPLICATION_SETTINGS_FILENAME))
        {
        }

        #endregion

        #region IApplicationSettings

        public string LastVersionNumber
        {
            get => Get<string>(null);
            set => Set(value);
        }

        #endregion
    }
}
