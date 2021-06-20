using System;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.Enums;
using ClipboardCanvas.EventArguments;

namespace ClipboardCanvas.ViewModels.UserControls.InAppNotifications
{
    public class InAppNotificationControlViewModel : ObservableObject
    {
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

        private InAppNotificationDisplayOptionsDataModel _NotificationOptions;
        public InAppNotificationDisplayOptionsDataModel NotificationOptions
        {
            get => _NotificationOptions;
            private set => SetProperty(ref _NotificationOptions, value);
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

        public InAppNotificationControlViewModel()
        {
            CheckShownButtons();

            this.NotificationOptions = new InAppNotificationDisplayOptionsDataModel();

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
            DismissNotification();
            OnInAppNotificationDismissedEvent?.Invoke(this, new InAppNotificationDismissedEventArgs(NotificationResult));
        }

        private void YesButtonClick()
        {
            NotificationResult = InAppNotificationButtonType.YesButton;
            DismissNotification();
            OnInAppNotificationDismissedEvent?.Invoke(this, new InAppNotificationDismissedEventArgs(NotificationResult));
        }

        private void OkButtonClick()
        {
            NotificationResult = InAppNotificationButtonType.OkButton;
            DismissNotification();
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

        public void ShowNotification(int miliseconds = 0)
        {
            NotificationOptions = new InAppNotificationDisplayOptionsDataModel()
            {
                show = true,
                miliseconds = miliseconds
            };
        }

        public void DismissNotification()
        {
            NotificationOptions = new InAppNotificationDisplayOptionsDataModel()
            {
                show = false
            };
        }

        #endregion
    }
}
