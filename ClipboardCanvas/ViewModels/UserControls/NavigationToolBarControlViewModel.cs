using System.Windows.Input;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;

using ClipboardCanvas.Models;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Services;
using ClipboardCanvas.ViewModels.Dialogs;
using ClipboardCanvas.ViewModels.UserControls.Autopaste;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class NavigationToolBarControlViewModel : ObservableObject, INavigationToolBarControlModel
    {
        #region Properties

        private IDialogService DialogService { get; } = Ioc.Default.GetService<IDialogService>();

        public INavigationControlModel NavigationControlModel { get; set; } = new NavigationControlViewModel();

        public ISuggestedActionsControlModel SuggestedActionsControlModel { get; set; } = new SuggestedActionsControlViewModel();

        public IAutopasteControlModel AutopasteControlModel => AutopasteControlViewModel;

        public AutopasteControlViewModel AutopasteControlViewModel { get; set; } = new AutopasteControlViewModel();

        private bool _IsStatusCenterButtonVisible;
        public bool IsStatusCenterButtonVisible
        {
            get => _IsStatusCenterButtonVisible;
            set => SetProperty(ref _IsStatusCenterButtonVisible, value);
        }

        #endregion

        #region Commands

        public ICommand OpenSettingsCommand { get; private set; }

        #endregion

        #region Constructor

        public NavigationToolBarControlViewModel()
        {
            // Create Commands
            OpenSettingsCommand = new AsyncRelayCommand(OpenSettings);
        }

        #endregion

        #region Command Implementation

        private async Task OpenSettings()
        {
            await DialogService.ShowDialog(new SettingsDialogViewModel());
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
