using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsGeneralPageViewModel : ObservableObject
    {
        #region Public Properties

        public bool UseInfiniteCanvasAsDefault
        {
            get => App.AppSettings.UserSettings.UseInfiniteCanvasAsDefault;
            set
            {
                if (value != App.AppSettings.UserSettings.UseInfiniteCanvasAsDefault)
                {
                    App.AppSettings.UserSettings.UseInfiniteCanvasAsDefault = value;

                    OnPropertyChanged(nameof(UseInfiniteCanvasAsDefault));
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
