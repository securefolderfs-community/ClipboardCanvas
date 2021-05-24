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
            get => $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";
        }

        #endregion

        #region Commands

        public ICommand ShowChangelogCommand { get; private set; }

        public ICommand SubmitFeedbackCommand { get; private set; }

        #endregion

        #region Constructor

        public SettingsAboutPageViewModel()
        {
            // Create commands
            ShowChangelogCommand = new RelayCommand(ShowChangelog);
            SubmitFeedbackCommand = new RelayCommand(SubmitFeedback);
        }

        #endregion

        #region Command Implementation

        private async void ShowChangelog()
        {
            // TODO: Open changelog dialog
            await Launcher.LaunchUriAsync(new Uri(@"https://github.com/d2dyno1/ClipboardCanvas"));
        }

        private async void SubmitFeedback()
        {
            await Launcher.LaunchUriAsync(new Uri(@"https://github.com/d2dyno1/ClipboardCanvas/issues"));
        }

        #endregion
    }
}
