using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;

using ClipboardCanvas.Services;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsPastingPageViewModel : ObservableObject
    {
        private IUserSettingsService UserSettings { get; } = Ioc.Default.GetService<IUserSettingsService>();

        public bool OpenNewCanvasOnPaste
        {
            get => UserSettings.OpenNewCanvasOnPaste;
            set
            {
                if (value != UserSettings.OpenNewCanvasOnPaste)
                {
                    UserSettings.OpenNewCanvasOnPaste = value;

                    OnPropertyChanged(nameof(OpenNewCanvasOnPaste));
                }
            }
        }

        public bool AlwaysPasteFilesAsReference
        {
            get => App.IsInRestrictedAccessMode ? false : UserSettings.AlwaysPasteFilesAsReference;
            set
            {
                if (value != UserSettings.AlwaysPasteFilesAsReference)
                {
                    UserSettings.AlwaysPasteFilesAsReference = value;

                    OnPropertyChanged(nameof(AlwaysPasteFilesAsReference));
                }
            }
        }

        public bool PrioritizeMarkdownOverText
        {
            get => UserSettings.PrioritizeMarkdownOverText;
            set
            {
                if (value != UserSettings.PrioritizeMarkdownOverText)
                {
                    UserSettings.PrioritizeMarkdownOverText = value;

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
            get => App.IsInRestrictedAccessMode;
        }

        public ICommand ShowReferenceFilesTeachingTipCommand { get; private set; }

        public SettingsPastingPageViewModel()
        {
            // Create commands
            ShowReferenceFilesTeachingTipCommand = new RelayCommand(ShowReferenceFilesTeachingTip);
        }

        private void ShowReferenceFilesTeachingTip()
        {
            IsReferenceFilesTeachingTipVisible = true;
        }
    }
}
