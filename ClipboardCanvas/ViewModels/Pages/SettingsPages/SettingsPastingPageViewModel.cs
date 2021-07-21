using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;

namespace ClipboardCanvas.ViewModels.Pages.SettingsPages
{
    public class SettingsPastingPageViewModel : ObservableObject
    {
        #region Public Properties

        private bool _IsReferenceFilesTeachingTipVisible;
        public bool IsReferenceFilesTeachingTipVisible
        {
            get => _IsReferenceFilesTeachingTipVisible;
            set => SetProperty(ref _IsReferenceFilesTeachingTipVisible, value);
        }

        public bool OpenNewCanvasOnPaste
        {
            get => App.AppSettings.UserSettings.OpenNewCanvasOnPaste;
            set
            {
                if (value != App.AppSettings.UserSettings.OpenNewCanvasOnPaste)
                {
                    App.AppSettings.UserSettings.OpenNewCanvasOnPaste = value;

                    OnPropertyChanged(nameof(OpenNewCanvasOnPaste));
                }
            }
        }

        public bool AlwaysPasteFilesAsReference
        {
            get => App.IsInRestrictedAccessMode ? false : App.AppSettings.UserSettings.AlwaysPasteFilesAsReference;
            set
            {
                if (value != App.AppSettings.UserSettings.AlwaysPasteFilesAsReference)
                {
                    App.AppSettings.UserSettings.AlwaysPasteFilesAsReference = value;

                    OnPropertyChanged(nameof(AlwaysPasteFilesAsReference));
                }
            }
        }

        public bool PrioritizeMarkdownOverText
        {
            get => App.AppSettings.UserSettings.PrioritizeMarkdownOverText;
            set
            {
                if (value != App.AppSettings.UserSettings.PrioritizeMarkdownOverText)
                {
                    App.AppSettings.UserSettings.PrioritizeMarkdownOverText = value;

                    OnPropertyChanged(nameof(PrioritizeMarkdownOverText));
                }
            }
        }

        public bool IsInRestrictedAccessMode
        {
            get => App.IsInRestrictedAccessMode;
        }

        #endregion

        #region Commands

        public ICommand ShowReferenceFilesTeachingTipCommand { get; private set; }

        #endregion

        #region Constructor

        public SettingsPastingPageViewModel()
        {
            // Create commands
            ShowReferenceFilesTeachingTipCommand = new RelayCommand(ShowReferenceFilesTeachingTip);
        }

        #endregion

        #region Command Implementation

        private void ShowReferenceFilesTeachingTip()
        {
            IsReferenceFilesTeachingTipVisible = true;
        }

        #endregion
    }
}
