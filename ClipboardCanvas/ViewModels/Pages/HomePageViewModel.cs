using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System.Threading.Tasks;

using ClipboardCanvas.Services;
using ClipboardCanvas.Serialization;

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
