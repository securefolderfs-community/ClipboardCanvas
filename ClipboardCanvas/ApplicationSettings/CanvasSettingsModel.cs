using ClipboardCanvas.ApplicationSettings.Interfaces;
using System.IO;
using Windows.Storage;

namespace ClipboardCanvas.ApplicationSettings
{
    public class CanvasSettingsModel : BaseJsonSettingsModel, ICanvasSettings
    {
        #region Constructor

        public CanvasSettingsModel()
            : base (Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.LocalSettings.SETTINGS_FOLDERNAME, Constants.LocalSettings.CANVAS_SETTINGS_FILENAME),
                  isCachingEnabled: true)
        {
        }

        #endregion

        #region ICanvasSettings

        public bool MediaCanvas_IsLoopingEnabled
        {
            get => Get(false);
            set => Set(value);
        }

        public double MediaCanvas_UniversalVolume
        {
            get => Get(100.0d);
            set => Set(value);
        }

        #endregion
    }
}
