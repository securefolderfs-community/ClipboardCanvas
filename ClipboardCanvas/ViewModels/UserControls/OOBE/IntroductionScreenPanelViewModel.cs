using System;
using System.Linq;
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

        private int ItemsCount => _view.ItemsCount;

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

        private double _Progress4;
        public double Progress4
        {
            get => _Progress4;
            set => SetProperty(ref _Progress4, value);
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

        private void UpdateTwoHalfProgress(int index, int count, params Action<double>[] progress)
        {
            int progressIndex = 0;
            int progressCount = progress.Count();
            bool progressHalf = false;

            // Fill
            for (int i = 0; i < count; i++)
            {
                if (i <= index)
                {
                    if (!progressHalf)
                    {
                        progress[progressIndex](50.0d);
                        progressHalf = true;
                    }
                    else
                    {
                        progress[progressIndex](100.0d);
                        progressHalf = false;
                        progressIndex++;
                    }
                }
                else
                {
                    if (!progressHalf)
                    {
                        // Unfill
                        progress[progressIndex](0.0d);
                        progressHalf = true;
                    }
                    else
                    {
                        progressHalf = false;
                        progressIndex++;
                    }
                }
            }
        }

        private void SelectedIndexChanged(int newIndex)
        {
            UpdateTwoHalfProgress(newIndex,
                _view?.ItemsCount ?? 0,
                (i1) => Progress1 = i1,
                (i2) => Progress2 = i2,
                (i3) => Progress3 = i3,
                (i4) => Progress4 = i4);

            CurrentStepText = $"{newIndex + 1}/{ItemsCount}";

            if (newIndex == (ItemsCount - 1))
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
