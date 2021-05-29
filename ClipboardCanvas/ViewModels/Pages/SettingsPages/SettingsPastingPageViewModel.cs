using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsPastingPageViewModel : ObservableObject
    {
        #region Public Properties

        public bool AlwaysPasteFilesAsReference
        {
            get => App.IsInRestrictedAccessMode ? false : App.AppSettings.UserSettings.AlwaysPasteFilesAsReference;
            set
            {
                if (value != App.AppSettings.UserSettings.AlwaysPasteFilesAsReference)
                {
                    App.AppSettings.UserSettings.AlwaysPasteFilesAsReference = value;

                    OnPropertyChanged(nameof(AlwaysPasteFilesAsReference));
                }
            }
        }

        public bool PrioritizeMarkdownOverText
        {
            get => App.AppSettings.UserSettings.PrioritizeMarkdownOverText;
            set
            {
                if (value != App.AppSettings.UserSettings.PrioritizeMarkdownOverText)
                {
                    App.AppSettings.UserSettings.PrioritizeMarkdownOverText = value;

                    OnPropertyChanged(nameof(PrioritizeMarkdownOverText));
                }
            }
        }

        public bool IsInRestrictedAccessMode
        {
            get => App.IsInRestrictedAccessMode;
        }

        #endregion
    }
}
