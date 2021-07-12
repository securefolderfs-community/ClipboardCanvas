using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;

using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Enums;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class NavigationToolBarControlViewModel : ObservableObject, INavigationToolBarControlModel
    {
        #region Private Members

        private readonly INavigationToolBarControlView _view;

        #endregion

        #region Public Properties

        public INavigationControlModel NavigationControlModel { get; set; } = new NavigationControlViewModel();

        public ISuggestedActionsControlModel SuggestedActionsControlModel { get; set; } = new SuggestedActionsControlViewModel();

        private bool _IsSettingsPaneOpened;
        public bool IsSettingsPaneOpened
        {
            get => _IsSettingsPaneOpened;
            set
            {
                if (SetProperty(ref _IsSettingsPaneOpened, value))
                {
                    OnPropertyChanged(nameof(SettingsButtonText));
                }
            }
        }

        public string SettingsButtonText
        {
            get => IsSettingsPaneOpened ? "Close Settings" : "Open Settings";
        }

        #endregion

        #region Commands

        public ICommand OpenOrCloseSettingsCommand { get; private set; }

        #endregion

        #region Constructor

        public NavigationToolBarControlViewModel(INavigationToolBarControlView view)
        {
            this._view = view;

            // Create Commands
            OpenOrCloseSettingsCommand = new RelayCommand(OpenOrCloseSettings);
        }

        #endregion

        #region Commands Implementation

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
