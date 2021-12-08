using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.Threading.Tasks;

using ClipboardCanvas.Services;
using ClipboardCanvas.EventArguments;

namespace ClipboardCanvas.ViewModels.Pages
{
    public class HomePageViewModel : ObservableObject, IDisposable
    {
        #region Properties

        private ITimelineService TimelineService { get; } = Ioc.Default.GetService<ITimelineService>();

        private IUserSettingsService UserSettingsService { get; } = Ioc.Default.GetService<IUserSettingsService>();

        public bool IsTimelineWidgetEnabled
        {
            get => UserSettingsService.ShowTimelineOnHomepage;
        }

        private bool _IsAutopasteTeachingTipShown;
        public bool IsAutopasteTeachingTipShown
        {
            get => _IsAutopasteTeachingTipShown;
            set => SetProperty(ref _IsAutopasteTeachingTipShown, value);
        }

        #endregion

        public HomePageViewModel()
        {
            // Hook events
            UserSettingsService.OnSettingChangedEvent += UserSettingsService_OnSettingChangedEvent;
        }

        public async Task LoadWidgets()
        {
            await TimelineService.LoadAllSectionsAsync();
        }

        public void OpenTeachingTip()
        {
            IsAutopasteTeachingTipShown = true;
        }

        private void UserSettingsService_OnSettingChangedEvent(object sender, SettingChangedEventArgs e)
        {
            if (e.settingName == nameof(UserSettingsService.ShowTimelineOnHomepage))
            {
                OnPropertyChanged(nameof(IsTimelineWidgetEnabled));
            }
        }

        #region IDisposable

        public void Dispose()
        {
            UserSettingsService.OnSettingChangedEvent -= UserSettingsService_OnSettingChangedEvent;
            TimelineService.UnloadAllSections();
        }

        #endregion
    }
}
