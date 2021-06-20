using System;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using ClipboardCanvas.Enums;
using ClipboardCanvas.EventArguments;
using Windows.UI.Xaml;

namespace ClipboardCanvas.ViewModels.UserControls.InAppNotifications
{
    public class InAppNotificationControlViewModel : ObservableObject, IDisposable
    {
        #region Private Members

        private readonly DispatcherTimer _progressTimer;

        private int _milisecondsPassed;

        private int _showMiliseconds;

        #endregion

        #region Public Properties

        public InAppNotificationButtonType NotificationResult { get; private set; }

        private InAppNotificationButtonType _ShownButtons;
        public InAppNotificationButtonType ShownButtons
        {
            get => _ShownButtons;
            set
            {
                if (_ShownButtons != value)
                {
                    _ShownButtons = value;
                    CheckShownButtons();
                }
            }
        }

        private bool _ShowNotification;
        public bool ShowNotification
        {
            get => _ShowNotification;
            private set => SetProperty(ref _ShowNotification, value);
        }

        private string _NotificationText;
        public string NotificationText
        {
            get => _NotificationText;
            set => SetProperty(ref _NotificationText, value);
        }

        private bool _OkButtonLoad;
        public bool OkButtonLoad
        {
            get => _OkButtonLoad;
            private set => SetProperty(ref _OkButtonLoad, value);
        }

        private bool _YesButtonLoad;
        public bool YesButtonLoad
        {
            get => _YesButtonLoad;
            private set => SetProperty(ref _YesButtonLoad, value);
        }

        private bool _NoButtonLoad;
        public bool NoButtonLoad
        {
            get => _NoButtonLoad;
            private set => SetProperty(ref _NoButtonLoad, value);
        }

        private bool _NotificationShowTimerProgressBarLoad;
        public bool NotificationShowTimerProgressBarLoad
        {
            get => _NotificationShowTimerProgressBarLoad;
            set => SetProperty(ref _NotificationShowTimerProgressBarLoad, value);
        }

        private double _NotificationShowTimerProgressBarValue;
        public double NotificationShowTimerProgressBarValue
        {
            get => _NotificationShowTimerProgressBarValue;
            set => SetProperty(ref _NotificationShowTimerProgressBarValue, value);
        }

        #endregion

        #region Events

        public event EventHandler<InAppNotificationDismissedEventArgs> OnInAppNotificationDismissedEvent;

        #endregion

        #region Commands

        public ICommand OkButtonClickCommand { get; private set; }

        public ICommand YesButtonClickCommand { get; private set; }

        public ICommand NoButtonClickCommand { get; private set; }

        #endregion

        #region Constructor

        public InAppNotificationControlViewModel(InAppNotificationButtonType shownButtons)
        {
            this.ShownButtons = shownButtons;
            CheckShownButtons();

            this._progressTimer = new DispatcherTimer();
            this._progressTimer.Interval = TimeSpan.FromMilliseconds(Constants.UI.Notifications.NOTIFICATION_PROGRESSBAR_REFRESH_INTERVAL);
            this._progressTimer.Tick += ProgressTimer_Tick;

            // Create commands
            OkButtonClickCommand = new RelayCommand(OkButtonClick);
            YesButtonClickCommand = new RelayCommand(YesButtonClick);
            NoButtonClickCommand = new RelayCommand(NoButtonClick);
        }

        #endregion

        #region Command Implementation

        private void NoButtonClick()
        {
            NotificationResult = InAppNotificationButtonType.NoButton;
            Dismiss();
            OnInAppNotificationDismissedEvent?.Invoke(this, new InAppNotificationDismissedEventArgs(NotificationResult));
        }

        private void YesButtonClick()
        {
            NotificationResult = InAppNotificationButtonType.YesButton;
            Dismiss();
            OnInAppNotificationDismissedEvent?.Invoke(this, new InAppNotificationDismissedEventArgs(NotificationResult));
        }

        private void OkButtonClick()
        {
            NotificationResult = InAppNotificationButtonType.OkButton;
            Dismiss();
            OnInAppNotificationDismissedEvent?.Invoke(this, new InAppNotificationDismissedEventArgs(NotificationResult));
        }

        #endregion

        #region Event Handlers

        private void ProgressTimer_Tick(object sender, object e)
        {
            _milisecondsPassed += Constants.UI.Notifications.NOTIFICATION_PROGRESSBAR_REFRESH_INTERVAL;

            double percentage = (double)_milisecondsPassed * 100.0d / (double)_showMiliseconds;
            NotificationShowTimerProgressBarValue = percentage;

            if (_milisecondsPassed >= _showMiliseconds)
            {
                _progressTimer.Stop();
                Dismiss();
            }
        }

        #endregion

        #region Private Helpers

        private void CheckShownButtons()
        {
            // For OK Button
            if (ShownButtons.HasFlag(InAppNotificationButtonType.OkButton))
            {
                OkButtonLoad = true;
            }
            else
            {
                OkButtonLoad = false;
            }

            // For Yes Button
            if (ShownButtons.HasFlag(InAppNotificationButtonType.YesButton))
            {
                YesButtonLoad = true;
            }
            else
            {
                YesButtonLoad = false;
            }

            // For No Button
            if (ShownButtons.HasFlag(InAppNotificationButtonType.NoButton))
            {
                NoButtonLoad = true;
            }
            else
            {
                NoButtonLoad = false;
            }
        }

        #endregion

        #region Public Helpers

        public void Show(int miliseconds = 0)
        {
            if (miliseconds > 0)
            {
                _milisecondsPassed = 0;
                _showMiliseconds = miliseconds;
                NotificationShowTimerProgressBarLoad = true;
                _progressTimer.Start();
            }

            ShowNotification = true;
        }

        public void Dismiss()
        {
            ShowNotification = false;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            this._progressTimer.Tick -= ProgressTimer_Tick;
        }

        #endregion
    }
}
