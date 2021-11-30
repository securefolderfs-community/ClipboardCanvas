using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System.Collections.ObjectModel;
using Windows.Globalization;

using ClipboardCanvas.Services;
using ClipboardCanvas.DataModels;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsGeneralPageViewModel : ObservableObject
    {
        private IUserSettingsService UserSettingsService { get; } = Ioc.Default.GetService<IUserSettingsService>();

        public ObservableCollection<AppLanguageModel> AppLanguages { get; set; }

        public SettingsGeneralPageViewModel()
        {
            AppLanguages = new ObservableCollection<AppLanguageModel>();

            foreach (var item in ApplicationLanguages.ManifestLanguages)
            {
                AppLanguages.Add(new AppLanguageModel(item));
            }
        }

        private int _SelectedLanguageIndex;
        public int SelectedLanguageIndex
        {
            get => _SelectedLanguageIndex;
            set
            {
                if (SetProperty(ref _SelectedLanguageIndex, value))
                {
                    UserSettingsService.AppLanguage = AppLanguages[value];

                    if (UserSettingsService.AppLanguage.Id != AppLanguages[value].Id)
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
