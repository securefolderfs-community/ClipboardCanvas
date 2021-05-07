using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsPastingPageViewModel : ObservableObject
    {
        #region Public Properties

        public bool AlwaysPasteFilesAsReference
        {
            get => App.AppSettings.UserSettings.AlwaysPasteFilesAsReference;
            set
            {
                if (value != App.AppSettings.UserSettings.AlwaysPasteFilesAsReference)
                {
                    App.AppSettings.UserSettings.AlwaysPasteFilesAsReference = value;

                    OnPropertyChanged(nameof(AlwaysPasteFilesAsReference));
                }
            }
        }

        #endregion
    }
}
