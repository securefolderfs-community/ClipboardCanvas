using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.System;
using ClipboardCanvas.ViewModels.Dialogs;
using ClipboardCanvas.Services;

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

        public ICommand OpenLogLocationCommand { get; private set; }

        public ICommand ShowChangeLogCommand { get; private set; }

        public ICommand SubmitFeedbackCommand { get; private set; }

        public ICommand OpenPrivacyPolicyCommand { get; private set; }

        #endregion

        #region Constructor

        public SettingsAboutPageViewModel()
        {
            // Create commands
            OpenLogLocationCommand = new AsyncRelayCommand(OpenLogLocation);
            ShowChangeLogCommand = new AsyncRelayCommand(ShowChangeLog);
            SubmitFeedbackCommand = new AsyncRelayCommand(SubmitFeedback);
            OpenPrivacyPolicyCommand = new AsyncRelayCommand(OpenPrivacyPolicy);
        }

        #endregion

        #region Command Implementation

        private async Task OpenPrivacyPolicy()
        {
            await Launcher.LaunchUriAsync(new Uri(@"https://github.com/d2dyno1/ClipboardCanvas/blob/master/Privacy.md"));
        }

        private async Task SubmitFeedback()
        {
            await Launcher.LaunchUriAsync(new Uri(@"https://github.com/d2dyno1/ClipboardCanvas/issues"));
        }

        private async Task ShowChangeLog()
        {
            await App.DialogService.ShowDialog(new UpdateChangeLogDialogViewModel());
        }

        private async Task OpenLogLocation()
        {
            await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder);
        }

        #endregion
    }
}
