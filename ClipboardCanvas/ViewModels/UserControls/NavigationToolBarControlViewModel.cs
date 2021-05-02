using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class NavigationToolBarControlViewModel : ObservableObject, INavigationToolBarControlModel
    {
        #region Private Members

        private readonly INavigationToolBarControlView _view;

        #endregion

        #region Public Properties

        public INavigationControlModel NavigationControlModel => _view?.NavigationControlModel;

        public IAdaptiveOptionsControlModel AdaptiveOptionsControlModel => _view?.AdaptiveOptionsControlModel;

        private bool _IsSettingsPaneOpened;
        public bool IsSettingsPaneOpened
        {
            get => _IsSettingsPaneOpened;
            set
            {
                if (SetProperty(ref _IsSettingsPaneOpened, value))
                {
                    OnPropertyChanged(nameof(SettingsHyperlinkText));
                }
            }
        }

        public string SettingsHyperlinkText
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

        #region IInstanceNotifyModel

        public void NotifyCurrentPageChanged(DisplayFrameNavigationDataModel navigationDataModel)
        {
            NavigationControlModel?.NotifyCurrentPageChanged(navigationDataModel);
        }

        #endregion
    }
}
