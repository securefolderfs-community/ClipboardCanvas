using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.System;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Windows.ApplicationModel.DataTransfer;
using Octokit;
using System.Linq;

using ClipboardCanvas.ViewModels.Dialogs;
using ClipboardCanvas.Services;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsAboutPageViewModel : ObservableObject
    {
        private bool _privacyPolicyTextLoaded;

        private readonly ISettingsAboutPageView _view;

        private IDialogService DialogService { get; } = Ioc.Default.GetService<IDialogService>();

        private IApplicationService ApplicationService { get; } = Ioc.Default.GetService<IApplicationService>();

        public string AppVersionText
        {
            get => $"Version: {ApplicationService.AppVersion}";
        }

        private bool _PrivacyPolicyProgressRingLoad;
        public bool PrivacyPolicyProgressRingLoad
        {
            get => _PrivacyPolicyProgressRingLoad;
            set => SetProperty(ref _PrivacyPolicyProgressRingLoad, value);
        }

        private bool _PrivacyPolicyLoadError;
        public bool PrivacyPolicyLoadError
        {
            get => _PrivacyPolicyLoadError;
            set => SetProperty(ref _PrivacyPolicyLoadError, value);
        }

        private string _PrivacyPolicyText;
        public string PrivacyPolicyText
        {
            get => _PrivacyPolicyText;
            set => SetProperty(ref _PrivacyPolicyText, value);
        }

        public ICommand LoadPrivacyPolicyCommand { get; private set; }

        public ICommand CopyVersionCommand { get; private set; }

        public ICommand OpenOnGitHubCommand { get; private set; }

        public ICommand OpenLogLocationCommand { get; private set; }

        public ICommand ShowIntroductionScreenCommand { get; private set; }

        public ICommand ShowChangeLogCommand { get; private set; }

        public ICommand SubmitFeedbackCommand { get; private set; }

        public ICommand OpenPrivacyPolicyCommand { get; private set; }

        public SettingsAboutPageViewModel(ISettingsAboutPageView view)
        {
            this._view = view;

            // Create commands
            LoadPrivacyPolicyCommand = new AsyncRelayCommand(LoadPrivacyPolicy);
            CopyVersionCommand = new RelayCommand(CopyVersion);
            OpenOnGitHubCommand = new AsyncRelayCommand(OpenOnGitHub);
            OpenPrivacyPolicyCommand = new AsyncRelayCommand(OpenPrivacyPolicy);
            OpenLogLocationCommand = new AsyncRelayCommand(OpenLogLocation);
            ShowIntroductionScreenCommand = new RelayCommand(ShowIntroductionScreen);
            ShowChangeLogCommand = new AsyncRelayCommand(ShowChangeLog);
            SubmitFeedbackCommand = new AsyncRelayCommand(SubmitFeedback);
        }

        private async Task LoadPrivacyPolicy()
        {
            if (_privacyPolicyTextLoaded)
            {
                return;
            }
            PrivacyPolicyLoadError = false;
            _privacyPolicyTextLoaded = true;

            try
            {
                PrivacyPolicyProgressRingLoad = true;

                const string owner = Constants.ClipboardCanvasRepository.REPOSITORY_OWNER;
                const string repositoryName = Constants.ClipboardCanvasRepository.REPOSITORY_NAME;

                const string privacyPolicyFileName = Constants.ClipboardCanvasRepository.PRIVACY_POLICY_FILENAME;

                GitHubClient client = new GitHubClient(new ProductHeaderValue(owner));
                var fileContents = await client.Repository.Content.GetAllContents(owner, repositoryName, privacyPolicyFileName);
                RepositoryContent content = fileContents.FirstOrDefault();

                if (content != null)
                {
                    string formatted = content.Content.Replace("\r\n", "\r\n\n").Replace("<br/>", "\n");
                    PrivacyPolicyText = formatted;
                }
                else
                {
                    PrivacyPolicyLoadError = true;
                }
            }
            catch
            {
                PrivacyPolicyLoadError = true;
                _privacyPolicyTextLoaded = false;
            }
            finally
            {
                PrivacyPolicyProgressRingLoad = false;
            }
        }

        private void CopyVersion()
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(AppVersionText);

            ClipboardHelpers.CopyDataPackage(dataPackage);
        }

        private async Task OpenOnGitHub()
        {
            await Launcher.LaunchUriAsync(new Uri(@"https://github.com/d2dyno1/ClipboardCanvas"));
        }

        private async Task OpenPrivacyPolicy()
        {
            await Launcher.LaunchUriAsync(new Uri(@"https://github.com/d2dyno1/ClipboardCanvas/blob/master/Privacy.md"));
        }

        private void ShowIntroductionScreen()
        {
            DialogService.CloseAllDialogs();
            _view.IntroductionPanelLoad = true;
        }

        private async Task SubmitFeedback()
        {
            await Launcher.LaunchUriAsync(new Uri(@"https://github.com/d2dyno1/ClipboardCanvas/issues"));
        }

        private async Task ShowChangeLog()
        {
            DialogService.CloseAllDialogs();
            await DialogService.ShowDialog(new UpdateChangeLogDialogViewModel());
        }

        private async Task OpenLogLocation()
        {
            SafeWrapper<StorageFile> exceptionLogFile = await StorageHelpers.GetExceptionLogFile();
            if (exceptionLogFile)
            {
                await StorageHelpers.OpenContainingFolder(exceptionLogFile.Result);
            }
            else
            {
                await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder);
            }
        }
    }
}
