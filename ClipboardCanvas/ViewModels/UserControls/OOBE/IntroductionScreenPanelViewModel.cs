using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Services;

namespace ClipboardCanvas.ViewModels.UserControls.OOBE
{
    public class IntroductionScreenPanelViewModel : ObservableObject, IIntroductionScreenPanelModel
    {
        #region Private Members

        private readonly IIntroductionScreenPaneView _view;

        private IApplicationSettingsService ApplicationSettingsService { get; } = Ioc.Default.GetService<IApplicationSettingsService>();

        #endregion

        #region Public Properties

        private int _SelectedIndex = 0;
        public int SelectedIndex
        {
            get => _SelectedIndex;
            set
            {
                if (SetProperty(ref _SelectedIndex, value))
                {
                    SelectedIndexChanged(value);
                }
            }
        }

        private string _CurrentStepText;
        public string CurrentStepText
        {
            get => _CurrentStepText;
            set => SetProperty(ref _CurrentStepText, value);
        }

        private double _Progress1;
        public double Progress1
        {
            get => _Progress1;
            set => SetProperty(ref _Progress1, value);
        }

        private double _Progress2;
        public double Progress2
        {
            get => _Progress2;
            set => SetProperty(ref _Progress2, value);
        }

        private double _Progress3;
        public double Progress3
        {
            get => _Progress3;
            set => SetProperty(ref _Progress3, value);
        }

        private bool _BeginButtonLoad;
        public bool BeginButtonLoad
        {
            get => _BeginButtonLoad;
            set => SetProperty(ref _BeginButtonLoad, value);
        }

        #endregion

        #region Commands

        public ICommand SkipCommand { get; private set; }

        public ICommand BeginCommand { get; private set; }

        #endregion

        #region Constructor

        public IntroductionScreenPanelViewModel(IIntroductionScreenPaneView view)
        {
            this._view = view;

            SelectedIndexChanged(0);

            // Create commands
            SkipCommand = new RelayCommand(FinishOff);
            BeginCommand = new RelayCommand(FinishOff);
        }

        #endregion

        #region Helpers

        private void SelectedIndexChanged(int newIndex)
        {
            switch (newIndex)
            {
                default:
                case 0:
                    {
                        Progress1 = 50.0d;
                        Progress2 = 0.0d;
                        Progress3 = 0.0d;
                        break;
                    }

                case 1:
                    {
                        Progress1 = 100.0d;
                        Progress2 = 0.0d;
                        Progress3 = 0.0d;
                        break;
                    }

                case 2:
                    {
                        Progress1 = 100.0d;
                        Progress2 = 50.0d;
                        Progress3 = 0.0d;
                        break;
                    }

                case 3:
                    {
                        Progress1 = 100.0d;
                        Progress2 = 100.0d;
                        Progress3 = 0.0d;
                        break;
                    }

                case 4:
                    {
                        Progress1 = 100.0d;
                        Progress2 = 100.0d;
                        Progress3 = 50.0d;
                        break;
                    }

                case 5:
                    {
                        Progress1 = 100.0d;
                        Progress2 = 100.0d;
                        Progress3 = 100.0d;
                        break;
                    }
            }

            CurrentStepText = $"{newIndex + 1}/6";

            if (newIndex == 5)
            {
                BeginButtonLoad = true;
            }
            else
            {
                BeginButtonLoad = false;
            }
        }

        public void FinishOff()
        {
            _view.IntroductionPanelLoad = false;
            ApplicationSettingsService.IsUserIntroduced = true;
        }

        #endregion
    }
}
