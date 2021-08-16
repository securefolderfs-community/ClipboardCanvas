using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

using ClipboardCanvas.Services;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsGeneralPageViewModel : ObservableObject
    {
        private IUserSettingsService UserSettings { get; } = Ioc.Default.GetService<IUserSettingsService>();

        public bool UseInfiniteCanvasAsDefault
        {
            get => UserSettings.UseInfiniteCanvasAsDefault;
            set
            {
                if (value != UserSettings.UseInfiniteCanvasAsDefault)
                {
                    UserSettings.UseInfiniteCanvasAsDefault = value;

                    OnPropertyChanged(nameof(UseInfiniteCanvasAsDefault));
                }
            }
        }

        public bool ShowDeleteConfirmationDialog
        {
            get => UserSettings.ShowDeleteConfirmationDialog;
            set
            {
                if (value != UserSettings.ShowDeleteConfirmationDialog)
                {
                    UserSettings.ShowDeleteConfirmationDialog = value;

                    OnPropertyChanged(nameof(ShowDeleteConfirmationDialog));
                }
            }
        }

        public bool DeletePermanentlyAsDefault
        {
            get => UserSettings.DeletePermanentlyAsDefault;
            set
            {
                if (value != UserSettings.DeletePermanentlyAsDefault)
                {
                    UserSettings.DeletePermanentlyAsDefault = value;

                    OnPropertyChanged(nameof(DeletePermanentlyAsDefault));
                }
            }
        }

        public bool ShowTimelineOnHomepage
        {
            get => UserSettings.ShowTimelineOnHomepage;
            set
            {
                if (value != UserSettings.ShowTimelineOnHomepage)
                {
                    UserSettings.ShowTimelineOnHomepage = value;

                    OnPropertyChanged(nameof(ShowTimelineOnHomepage));
                }
            }
        }
    }
}
