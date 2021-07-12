using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;

using ClipboardCanvas.Models;
using ClipboardCanvas.Enums;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class NavigationToolBarControlViewModel : ObservableObject, INavigationToolBarControlModel
    {
        #region Public Properties

        public INavigationControlModel NavigationControlModel { get; set; } = new NavigationControlViewModel();

        public ISuggestedActionsControlModel SuggestedActionsControlModel { get; set; } = new SuggestedActionsControlViewModel();

        private bool _IsSettingsPaneOpened;
        public bool IsSettingsPaneOpened
        {
            get => _IsSettingsPaneOpened;
            set => SetProperty(ref _IsSettingsPaneOpened, value);
        }

        #endregion

        #region Commands

        public ICommand OpenOrCloseSettingsCommand { get; private set; }

        #endregion

        #region Constructor

        public NavigationToolBarControlViewModel()
        {
            // Create Commands
            OpenOrCloseSettingsCommand = new RelayCommand(OpenOrCloseSettings);
        }

        #endregion

        #region Command Implementation

        private void OpenOrCloseSettings()
        {
            IsSettingsPaneOpened = !IsSettingsPaneOpened;
        }

        #endregion

        #region INavigationToolBarControlModel

        public void NotifyCurrentPageChanged(DisplayPageType pageType)
        {
            NavigationControlModel?.NotifyCurrentPageChanged(pageType);
        }

        #endregion
    }
}
