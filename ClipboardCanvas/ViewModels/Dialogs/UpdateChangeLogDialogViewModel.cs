using ClipboardCanvas.Helpers;
using ClipboardCanvas.ViewModels.UserControls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.ViewModels.Dialogs
{
    public class UpdateChangeLogDialogViewModel : ObservableObject
    {
        #region Private Members

        private bool _dataFetchedSuccessfully = false;

        #endregion

        #region Public Properties

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

        #region Public Helpers

        public async Task LoadUpdateDataFromGitHub()
        {
            IsLoadingData = true;
            string currentVersion = App.AppVersion;

            try
            {
                string owner = "d2dyno";
                string repositoryName = "ClipboardCanvas";

                // Get all releases
                GitHubClient client = new GitHubClient(new ProductHeaderValue(owner));
                IReadOnlyList<Release> releases = await client.Repository.Release.GetAll(owner, repositoryName);

                List<(string Name, string Body)> preparedReleases = new List<(string Name, string Body)>();

                // Prepare releases
                foreach (var item in releases)
                {
                    string itemVersion = item.TagName;
                    itemVersion = itemVersion.Replace("v", string.Empty);

                    if (!item.Draft)
                    {
                        if (VersionHelpers.IsVersionBiggerThan(itemVersion, currentVersion))
                        {
                            preparedReleases.Add((item.Name, item.Body));
                        }
                        else if (VersionHelpers.IsVersionSmallerThan(itemVersion, currentVersion))
                        {
                            break;
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
                App.ExceptionLogger.Log(e.Message);
            }

            if (!_dataFetchedSuccessfully)
            {
                // Getting data failed, display fallback update info
                _UpdateMarkdownInfoText += $"Update {currentVersion}";
                _UpdateMarkdownInfoText += "\n---";
                _UpdateMarkdownInfoText += "\n";
                _UpdateMarkdownInfoText += $"Clipboard Canvas has been updated to version {currentVersion}.";
                _UpdateMarkdownInfoText += "\n";
                _UpdateMarkdownInfoText += "\n";
                _UpdateMarkdownInfoText += "\n";
                _UpdateMarkdownInfoText += "This update may include but is not limited to:";
                _UpdateMarkdownInfoText += "\n";
                _UpdateMarkdownInfoText += "\r\n\n- Stability and performance improvements";
                _UpdateMarkdownInfoText += "\r\n\n- New and/or enhanced features";
                _UpdateMarkdownInfoText += "\r\n\n- Bug fixes";

                IsLoadingData = false;
                UpdateMarkdownLoad = true;

                // Update the UI
                OnPropertyChanged(nameof(UpdateMarkdownInfoText));
            }
        }

        #endregion
    }
}
