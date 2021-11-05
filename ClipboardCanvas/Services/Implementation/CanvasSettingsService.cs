using System.IO;
using Windows.Storage;

using ClipboardCanvas.Models.JsonSettings;

namespace ClipboardCanvas.Services
{
    public class CanvasSettingsService : BaseJsonSettingsModel, ICanvasSettingsService
    {
        #region Constructor

        public CanvasSettingsService()
            : base (Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.LocalSettings.SETTINGS_FOLDERNAME, Constants.LocalSettings.CANVAS_SETTINGS_FILENAME),
                  isCachingEnabled: true)
        {
        }

        #endregion

        #region ICanvasSettings

        public bool MediaCanvas_IsLoopingEnabled
        {
            get => Get<bool>(false);
            set => Set(value);
        }

        public double MediaCanvas_UniversalVolume
        {
            get => Get<double>(1.0d);
            set => Set(value);
        }

        #endregion
    }
}
