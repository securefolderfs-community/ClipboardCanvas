using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.System;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsAboutPageViewModel : ObservableObject
    {
        #region Public Properties

        public string VersionNumber
        {
            get => App.AppVersion;
        }

        #endregion

        #region Commands

        public ICommand ShowChangeLogCommand { get; private set; }

        public ICommand SubmitFeedbackCommand { get; private set; }

        public ICommand OpenPrivacyPolicyCommand { get; private set; }

        #endregion

        #region Constructor

        public SettingsAboutPageViewModel()
        {
            // Create commands
            ShowChangeLogCommand = new RelayCommand(ShowChangeLog);
            SubmitFeedbackCommand = new RelayCommand(SubmitFeedback);
            OpenPrivacyPolicyCommand = new RelayCommand(OpenPrivacyPolicy);
        }

        #endregion

        #region Command Implementation

        private async void OpenPrivacyPolicy()
        {
            await Launcher.LaunchUriAsync(new Uri(@"https://github.com/d2dyno1/ClipboardCanvas/blob/master/Privacy.md"));
        }

        private async void SubmitFeedback()
        {
            await Launcher.LaunchUriAsync(new Uri(@"https://github.com/d2dyno1/ClipboardCanvas/issues"));
        }

        private async void ShowChangeLog()
        {
            // TODO: Open changelog dialog
            await Launcher.LaunchUriAsync(new Uri(@"https://github.com/d2dyno1/ClipboardCanvas"));
        }


        #endregion
    }
}
