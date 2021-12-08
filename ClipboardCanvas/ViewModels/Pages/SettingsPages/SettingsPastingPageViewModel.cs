using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

using ClipboardCanvas.Services;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsPastingPageViewModel : ObservableObject
    {
        private IUserSettingsService UserSettingsService { get; } = Ioc.Default.GetService<IUserSettingsService>();

        private IApplicationService ApplicationService { get; } = Ioc.Default.GetService<IApplicationService>();

        public bool OpenNewCanvasOnPaste
        {
            get => UserSettingsService.OpenNewCanvasOnPaste;
            set
            {
                if (value != UserSettingsService.OpenNewCanvasOnPaste)
                {
                    UserSettingsService.OpenNewCanvasOnPaste = value;

                    OnPropertyChanged(nameof(OpenNewCanvasOnPaste));
                }
            }
        }

        public bool AlwaysPasteFilesAsReference
        {
            get => ApplicationService.IsInRestrictedAccessMode ? false : UserSettingsService.AlwaysPasteFilesAsReference;
            set
            {
                if (value != UserSettingsService.AlwaysPasteFilesAsReference)
                {
                    UserSettingsService.AlwaysPasteFilesAsReference = value;

                    OnPropertyChanged(nameof(AlwaysPasteFilesAsReference));
                }
            }
        }

        public bool PrioritizeMarkdownOverText
        {
            get => UserSettingsService.PrioritizeMarkdownOverText;
            set
            {
                if (value != UserSettingsService.PrioritizeMarkdownOverText)
                {
                    UserSettingsService.PrioritizeMarkdownOverText = value;

                    OnPropertyChanged(nameof(PrioritizeMarkdownOverText));
                }
            }
        }

        private bool _IsReferenceFilesTeachingTipVisible;
        public bool IsReferenceFilesTeachingTipVisible
        {
            get => _IsReferenceFilesTeachingTipVisible;
            set => SetProperty(ref _IsReferenceFilesTeachingTipVisible, value);
        }

        public bool IsInRestrictedAccessMode
        {
            get => ApplicationService.IsInRestrictedAccessMode;
        }

        public ICommand ShowReferenceFilesTeachingTipCommand { get; private set; }

        public SettingsPastingPageViewModel()
        {
            // Create commands
            ShowReferenceFilesTeachingTipCommand = new RelayCommand(ShowReferenceFilesTeachingTip);
        }

        private void ShowReferenceFilesTeachingTip()
        {
            IsReferenceFilesTeachingTipVisible = !IsReferenceFilesTeachingTipVisible;
        }
    }
}
