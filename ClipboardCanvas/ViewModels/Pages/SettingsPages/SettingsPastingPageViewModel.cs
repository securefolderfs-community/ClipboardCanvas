using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsPastingPageViewModel : ObservableObject
    {
        #region Public Properties

        public bool CopyLargeItemsDirectlyToCollection
        {
            get => App.AppSettings.UserSettings.CopyLargeItemsDirectlyToCollection;
            set
            {
                if (value != App.AppSettings.UserSettings.CopyLargeItemsDirectlyToCollection)
                {
                    App.AppSettings.UserSettings.CopyLargeItemsDirectlyToCollection = value;

                    OnPropertyChanged(nameof(CopyLargeItemsDirectlyToCollection));
                }
            }
        }

        #endregion
    }
}
