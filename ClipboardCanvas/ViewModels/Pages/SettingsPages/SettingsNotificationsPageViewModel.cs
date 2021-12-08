using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;

using ClipboardCanvas.Services;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsNotificationsPageViewModel : ObservableObject
    {
        private IUserSettingsService UserSettingsService { get; } = Ioc.Default.GetService<IUserSettingsService>();

        public bool PushErrorNotification
        {
            get => UserSettingsService.PushErrorNotification;
            set
            {
                if (value != UserSettingsService.PushErrorNotification)
                {
                    UserSettingsService.PushErrorNotification = value;

                    OnPropertyChanged();
                }
            }
        }

        public bool PushAutopasteNotification
        {
            get => UserSettingsService.PushAutopasteNotification;
            set
            {
                if (value != UserSettingsService.PushAutopasteNotification)
                {
                    UserSettingsService.PushAutopasteNotification = value;

                    OnPropertyChanged();
                }
            }
        }

        public bool PushAutopasteFailedNotification
        {
            get => UserSettingsService.PushAutopasteFailedNotification;
            set
            {
                if (value != UserSettingsService.PushAutopasteFailedNotification)
                {
                    UserSettingsService.PushAutopasteFailedNotification = value;

                    OnPropertyChanged();
                }
            }
        }
    }
}
