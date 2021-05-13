using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsAboutPageViewModel : ObservableObject
    {
        #region Public Properties

        public string VersionNumber
        {
            get => "1.0.0.0";
            //get => $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";
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

        private void ShowChangelog()
        {

        }

        private void SubmitFeedback()
        {

        }

        #endregion
    }
}
