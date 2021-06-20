using System;
using ClipboardCanvas.EventArguments;

namespace ClipboardCanvas.ViewModels.UserControls.InAppNotifications
{
    public interface IInAppNotification
    {
        InAppNotificationControlViewModel ViewModel { get; set; }

        event EventHandler<InAppNotificationDismissedEventArgs> OnInAppNotificationDismissedEvent;

        void Show(int miliseconds = 0);

        void Dismiss();
    }
}
