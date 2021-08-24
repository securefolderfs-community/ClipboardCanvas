using System;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Threading.Tasks;
using Windows.UI.Xaml;

using ClipboardCanvas.Enums;
using ClipboardCanvas.EventArguments;

namespace ClipboardCanvas.ViewModels.UserControls.InAppNotifications
{
    public class InAppNotificationControlViewModel : ObservableObject, IDisposable
    {
        #region Private Members

        private readonly DispatcherTimer _timer;

        private int _passedMilliseconds;

        private int _showMilliseconds;

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
            this._timer = new DispatcherTimer();
            this._timer.Interval = TimeSpan.FromMilliseconds(Constants.UI.Notifications.NOTIFICATION_PROGRESSBAR_REFRESH_INTERVAL);
            this._timer.Tick += Timer_Tick;

            CheckShownButtons();

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

        private void Timer_Tick(object sender, object e)
        {
            // Increase the passed milliseconds by a constant value
            _passedMilliseconds += Constants.UI.Notifications.NOTIFICATION_PROGRESSBAR_REFRESH_INTERVAL;

            // Get progress
            double percentage = (double)_passedMilliseconds * 100.0d / (double)_showMilliseconds;
            NotificationShowTimerProgressBarValue = percentage;

            if (_passedMilliseconds >= _showMilliseconds)
            {
                Dismiss();
            }
        }

        #endregion

        #region Helpers

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

        public void Show(int milliseconds = 0)
        {
            Dismiss(); // Dismiss the last notification if there was one

            if (milliseconds > 0)
            {
                milliseconds = (int)(milliseconds * 0.6); // We lower down the time because the timer is slow and we need to account for an overhead
                _showMilliseconds = milliseconds;
                NotificationShowTimerProgressBarLoad = true;
                this._timer.Start();
            }

            ShowNotification = true;
        }

        public void Dismiss()
        {
            _timer.Stop();
            NotificationShowTimerProgressBarValue = 0.0d;
            NotificationShowTimerProgressBarLoad = false;
            _showMilliseconds = 0;
            _passedMilliseconds = 0;
            ShowNotification = false;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            this._timer.Tick -= Timer_Tick;
        }

        #endregion
    }
}
