using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Octokit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.System;
using CommunityToolkit.Mvvm.DependencyInjection;
using ClipboardCanvas.GlobalizationExtensions;

using ClipboardCanvas.Helpers;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.Services;

namespace ClipboardCanvas.ViewModels.Dialogs
{
    public class UpdateChangeLogDialogViewModel : ObservableObject
    {
        #region Private Members

        private bool _dataFetchedSuccessfully = false;

        #endregion

        #region Properties

        private IApplicationService ApplicationService { get; } = Ioc.Default.GetService<IApplicationService>();

        private bool _IsLoadingData;
        public bool IsLoadingData
        {
            get => _IsLoadingData;
            set => SetProperty(ref _IsLoadingData, value);
        }

        private string _UpdateMarkdownInfoText = string.Empty;
        public string UpdateMarkdownInfoText
        {
            get => _UpdateMarkdownInfoText;
            set => SetProperty(ref _UpdateMarkdownInfoText, value);
        }

        private bool _UpdateMarkdownLoad;
        public bool UpdateMarkdownLoad
        {
            get => _UpdateMarkdownLoad;
            set => SetProperty(ref _UpdateMarkdownLoad, value);
        }

        #endregion

        #region Commands

        public ICommand OpenReleasesOnGitHubCommand { get; private set; }

        #endregion

        #region Constructor

        public UpdateChangeLogDialogViewModel()
        {
            // Create commands
            OpenReleasesOnGitHubCommand = new AsyncRelayCommand(OpenReleasesOnGitHub);
        }

        #endregion

        #region Command Implementation

        private async Task OpenReleasesOnGitHub()
        {
            await Launcher.LaunchUriAsync(new Uri(@"https://github.com/d2dyno1/ClipboardCanvas/releases"));
        }

        #endregion

        #region Public Helpers

        public async Task LoadUpdateDataFromGitHub(bool onlyCompareCurrent = false)
        {
            IsLoadingData = true;
            string currentVersion = ApplicationService.AppVersion;
            List<(string Name, string Body)> preparedReleases = null;

            try
            {
                const string owner = Constants.ClipboardCanvasRepository.REPOSITORY_OWNER;
                const string repositoryName = Constants.ClipboardCanvasRepository.REPOSITORY_NAME;

                // Get all releases
                GitHubClient client = new GitHubClient(new ProductHeaderValue(owner));
                IReadOnlyList<Release> releases = await client.Repository.Release.GetAll(owner, repositoryName);

                preparedReleases = new List<(string Name, string Body)>();

                // Prepare releases
                foreach (var item in releases)
                {
                    string itemVersion = item.TagName;
                    itemVersion = itemVersion.Replace("v", string.Empty, StringComparison.OrdinalIgnoreCase);

                    if (!item.Draft)
                    {
                        if (onlyCompareCurrent)
                        {
                            if (VersionHelpers.IsVersionEqualTo(itemVersion, currentVersion))
                            {
                                preparedReleases.Add((item.Name, item.Body));
                                break;
                            }
                        }
                        else
                        {
                            if (VersionHelpers.IsVersionBiggerThanOrEqual(itemVersion, currentVersion))
                            {
                                preparedReleases.Add((item.Name, item.Body));
                            }
                            else if (VersionHelpers.IsVersionSmallerThan(itemVersion, currentVersion))
                            {
                                break;
                            }
                        }
                    }
                }

                _dataFetchedSuccessfully = true;

                // Compile update string
                foreach (var item in preparedReleases)
                {
                    _UpdateMarkdownInfoText += item.Name;
                    _UpdateMarkdownInfoText += "\n---";
                    _UpdateMarkdownInfoText += "\n";
                    _UpdateMarkdownInfoText += item.Body.Replace("\r\n", "\r\n\n");
                    _UpdateMarkdownInfoText += "\n";
                    _UpdateMarkdownInfoText += "\n";
                }

                IsLoadingData = false;
                UpdateMarkdownLoad = true;

                // Update the UI
                OnPropertyChanged(nameof(UpdateMarkdownInfoText));
            }
            catch (Exception e)
            {
                _dataFetchedSuccessfully = false;
            }

            if (!_dataFetchedSuccessfully || preparedReleases.IsEmpty())
            {
                // Getting data failed, display fallback update info
                _UpdateMarkdownInfoText += $"Error".GetLocalized();
                _UpdateMarkdownInfoText += "\n---";
                _UpdateMarkdownInfoText += "\n";
                _UpdateMarkdownInfoText += "ChangelogFetchingError".GetLocalized();

                IsLoadingData = false;
                UpdateMarkdownLoad = true;

                // Update the UI
                OnPropertyChanged(nameof(UpdateMarkdownInfoText));
            }
        }

        #endregion
    }
}
