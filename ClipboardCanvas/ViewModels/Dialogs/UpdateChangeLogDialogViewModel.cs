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
        #region Public Properties

        public ObservableCollection<UpdateChangeLogItemViewModel> Items { get; private set; }

        public bool IsLoadingData { get; private set; }

        #endregion

        #region Constructor

        public UpdateChangeLogDialogViewModel()
        {
            Items = new ObservableCollection<UpdateChangeLogItemViewModel>();
        }

        #endregion

        #region Public Helpers

        public async Task LoadUpdateDataFromGitHub()
        {
            IsLoadingData = true;

            try
            {
                string owner = "d2dyno";
                string repositoryName = "ClipboardCanvas";

                GitHubClient client = new GitHubClient(new ProductHeaderValue(owner));
                IReadOnlyList<Release> releases = await client.Repository.Release.GetAll(owner, repositoryName);

                string currentVersion = App.AppVersion;

                List<(string Name, string Body)> preparedReleases = releases
                    .Where((item) => !item.Draft && ApplicationHelpers.IsVersionBiggerThan(item.TagName, currentVersion)) // Check if not draft and if tag version is bigger than current version
                    .Select((item) => (item.Name, item.Body)).ToList(); // Compile a list

            }
            catch (Exception e)
            {
                App.ExceptionLogger.Log(e.Message);
            }

            IsLoadingData = false;
        }

        #endregion
    }
}
