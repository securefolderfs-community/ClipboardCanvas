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

        public bool ShowDeleteConfirmationDialog
        {
            get => App.AppSettings.UserSettings.ShowDeleteConfirmationDialog;
            set
            {
                if (value != App.AppSettings.UserSettings.ShowDeleteConfirmationDialog)
                {
                    App.AppSettings.UserSettings.ShowDeleteConfirmationDialog = value;

                    OnPropertyChanged(nameof(ShowDeleteConfirmationDialog));
                }
            }
        }

        #endregion
    }
}
