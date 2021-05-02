using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsGeneralPageViewModel : ObservableObject
    {
        #region Public Properties

        public bool OpenNewCanvasOnPaste
        {
            get => App.AppSettings.UserSettings.OpenNewCanvasOnPaste;
            set
            {
                if (value != App.AppSettings.UserSettings.OpenNewCanvasOnPaste)
                {
                    App.AppSettings.UserSettings.OpenNewCanvasOnPaste = value;

                    OnPropertyChanged(nameof(OpenNewCanvasOnPaste));
                }
            }
        }

        #endregion
    }
}
