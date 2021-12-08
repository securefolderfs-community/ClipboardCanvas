using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
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

                    if (ApplicationService.CurrentAppLanguage.Id != AppLanguages[value].Id)
                    {
                        RestartRequiredLoad = true;
                    }
                    else
                    {
                        RestartRequiredLoad = false;
                    }
                }
            }
        }

        private bool _RestartRequiredLoad;
        public bool RestartRequiredLoad
        {
            get => _RestartRequiredLoad;
            set => SetProperty(ref _RestartRequiredLoad, value);
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
