using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

using ClipboardCanvas.Services;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsNotificationsPageViewModel : ObservableObject
    {
        private IUserSettingsService UserSettings { get; } = Ioc.Default.GetService<IUserSettingsService>();

        public bool PushErrorNotification
        {
            get => UserSettings.PushErrorNotification;
            set
            {
                if (value != UserSettings.PushErrorNotification)
                {
                    UserSettings.PushErrorNotification = value;

                    OnPropertyChanged(nameof(PushErrorNotification));
                }
            }
        }
    }
}
