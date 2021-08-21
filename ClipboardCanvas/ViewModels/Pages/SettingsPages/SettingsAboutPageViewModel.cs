using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.System;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

using ClipboardCanvas.ViewModels.Dialogs;
using ClipboardCanvas.Services;
using ClipboardCanvas.ModelViews;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsAboutPageViewModel : ObservableObject
    {
        private ISettingsAboutPageView _view;

        private IDialogService DialogService { get; } = Ioc.Default.GetService<IDialogService>();

        public string VersionNumber
        {
            get => App.AppVersion;
        }

        public ICommand OpenLogLocationCommand { get; private set; }

        public ICommand ShowIntroductionScreenCommand { get; private set; }

        public ICommand ShowChangeLogCommand { get; private set; }

        public ICommand SubmitFeedbackCommand { get; private set; }

        public ICommand OpenPrivacyPolicyCommand { get; private set; }

        public SettingsAboutPageViewModel(ISettingsAboutPageView view)
        {
            this._view = view;

            // Create commands
            OpenLogLocationCommand = new AsyncRelayCommand(OpenLogLocation);
            ShowIntroductionScreenCommand = new RelayCommand(ShowIntroductionScreen);
            ShowChangeLogCommand = new AsyncRelayCommand(ShowChangeLog);
            SubmitFeedbackCommand = new AsyncRelayCommand(SubmitFeedback);
            OpenPrivacyPolicyCommand = new AsyncRelayCommand(OpenPrivacyPolicy);
        }

        private async Task OpenPrivacyPolicy()
        {
            await Launcher.LaunchUriAsync(new Uri(@"https://github.com/d2dyno1/ClipboardCanvas/blob/master/Privacy.md"));
        }

        private void ShowIntroductionScreen()
        {
            _view.IntroductionPanelLoad = true;
        }

        private async Task SubmitFeedback()
        {
            await Launcher.LaunchUriAsync(new Uri(@"https://github.com/d2dyno1/ClipboardCanvas/issues"));
        }

        private async Task ShowChangeLog()
        {
            await DialogService.ShowDialog(new UpdateChangeLogDialogViewModel());
        }

        private async Task OpenLogLocation()
        {
            await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder);
        }
    }
}
