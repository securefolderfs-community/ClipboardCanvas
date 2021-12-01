using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System.Collections.ObjectModel;

using ClipboardCanvas.Services;
using ClipboardCanvas.DataModels;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsGeneralPageViewModel : ObservableObject
    {
        private IUserSettingsService UserSettingsService { get; } = Ioc.Default.GetService<IUserSettingsService>();

        private IApplicationService ApplicationService { get; } = Ioc.Default.GetService<IApplicationService>();

        public ObservableCollection<AppLanguageModel> AppLanguages { get; set; }

        public SettingsGeneralPageViewModel()
        {
            AppLanguages = new ObservableCollection<AppLanguageModel>(ApplicationService.AppLanguages);
            _SelectedLanguageIndex = ApplicationService.AppLanguages.IndexOf(ApplicationService.AppLanguage);
        }

        private int _SelectedLanguageIndex;
        public int SelectedLanguageIndex
        {
            get => _SelectedLanguageIndex;
            set
            {
                if (SetProperty(ref _SelectedLanguageIndex, value))
                {
                    ApplicationService.AppLanguage = AppLanguages[value];

                    if (ApplicationService.AppLanguage.Id != AppLanguages[value].Id)
                    {
                        //ShowRestartControl = true;
                    }
                    else
                    {
                        //ShowRestartControl = false;
                    }
                }
            }
        }

        public bool UseInfiniteCanvasAsDefault
        {
            get => UserSettingsService.UseInfiniteCanvasAsDefault;
            set
            {
                if (value != UserSettingsService.UseInfiniteCanvasAsDefault)
                {
                    UserSettingsService.UseInfiniteCanvasAsDefault = value;

                    OnPropertyChanged(nameof(UseInfiniteCanvasAsDefault));
                }
            }
        }

        public bool ShowDeleteConfirmationDialog
        {
            get => UserSettingsService.ShowDeleteConfirmationDialog;
            set
            {
                if (value != UserSettingsService.ShowDeleteConfirmationDialog)
                {
                    UserSettingsService.ShowDeleteConfirmationDialog = value;

                    OnPropertyChanged(nameof(ShowDeleteConfirmationDialog));
                }
            }
        }

        public bool DeletePermanentlyAsDefault
        {
            get => UserSettingsService.DeletePermanentlyAsDefault;
            set
            {
                if (value != UserSettingsService.DeletePermanentlyAsDefault)
                {
                    UserSettingsService.DeletePermanentlyAsDefault = value;

                    OnPropertyChanged(nameof(DeletePermanentlyAsDefault));
                }
            }
        }

        public bool ShowTimelineOnHomepage
        {
            get => UserSettingsService.ShowTimelineOnHomepage;
            set
            {
                if (value != UserSettingsService.ShowTimelineOnHomepage)
                {
                    UserSettingsService.ShowTimelineOnHomepage = value;

                    OnPropertyChanged(nameof(ShowTimelineOnHomepage));
                }
            }
        }
    }
}
