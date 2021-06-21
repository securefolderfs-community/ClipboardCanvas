using System;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using ClipboardCanvas.Enums;
using ClipboardCanvas.EventArguments;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ClipboardCanvas.ViewModels.UserControls.InAppNotifications
{
    public class InAppNotificationControlViewModel : ObservableObject
    {
        #region Private Members

        private bool _showTimerCancelled;

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

        public async Task Show(int milliseconds = 0)
        {
            if (ShowNotification) // Don't show a notification when there's one shown already
            {
                return;
            }

            if (milliseconds > 0)
            {
                _showTimerCancelled = false;
                ShowNotification = true;
                NotificationShowTimerProgressBarValue = 0.0d;
                NotificationShowTimerProgressBarLoad = true;

                int passedMilliseconds = 0;
                while (passedMilliseconds < milliseconds)
                {
                    if (_showTimerCancelled) // Check if cancelled
                    {
                        break;
                    }

                    // Increase the passed milliseconds by a constant value
                    passedMilliseconds += Constants.UI.Notifications.NOTIFICATION_PROGRESSBAR_REFRESH_INTERVAL;

                    // Get progressbar percentage
                    double percentage = (double)passedMilliseconds * 100.0d / (double)milliseconds;
                    NotificationShowTimerProgressBarValue = percentage;

                    await Task.Delay(Constants.UI.Notifications.NOTIFICATION_PROGRESSBAR_REFRESH_INTERVAL);
                }

                Dismiss();
            }
            else
            {
                ShowNotification = true;
            }
        }

        public void Dismiss()
        {
            _showTimerCancelled = true;
            ShowNotification = false;
        }

        #endregion
    }
}
