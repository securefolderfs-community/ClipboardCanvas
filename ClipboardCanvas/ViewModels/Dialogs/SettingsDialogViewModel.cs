using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;

using ClipboardCanvas.DataModels.Navigation;
using ClipboardCanvas.Enums;
using ClipboardCanvas.ModelViews;

namespace ClipboardCanvas.ViewModels.Dialogs
{
    public class SettingsDialogViewModel : ObservableObject
    {
        private SettingsFrameNavigationDataModel _CurrentPageNavigation;
        public SettingsFrameNavigationDataModel CurrentPageNavigation
        {
            get => _CurrentPageNavigation;
            private set => SetProperty(ref _CurrentPageNavigation, value);
        }

        public ISettingsDialogView View { get; set; }

        public ICommand CloseDialogCommand { get; private set; }

        public SettingsDialogViewModel()
        {
            CurrentPageNavigation = new SettingsFrameNavigationDataModel(SettingsPageType.General);

            // Create commands
            CloseDialogCommand = new RelayCommand(CloseDialog);
        }

        private void CloseDialog()
        {
            this.View?.CloseDialog();
        }

        public void UpdateNavigation(SettingsPageType settingsPage)
        {
            CurrentPageNavigation = new SettingsFrameNavigationDataModel(settingsPage);
        }
    }
}
