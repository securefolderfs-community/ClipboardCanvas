using ClipboardCanvas.ApplicationSettings.Interfaces;
using System.IO;
using Windows.Storage;

namespace ClipboardCanvas.ApplicationSettings
{
    public class CanvasSettingsModel : BaseJsonSettingsModel, ICanvasSettings
    {
        #region Constructor

        public CanvasSettingsModel()
            : base (Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.LocalSettings.SETTINGS_FOLDERNAME, Constants.LocalSettings.CANVAS_SETTINGS_FILENAME))
        {
        }

        #endregion

        public bool MediaCanvas_IsLoopingEnabled
        {
            get => Get(false);
            set => Set(value);
        }
    }
}
