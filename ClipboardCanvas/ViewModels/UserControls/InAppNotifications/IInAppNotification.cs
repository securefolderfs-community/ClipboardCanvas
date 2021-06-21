using System;
using System.Threading.Tasks;
using ClipboardCanvas.EventArguments;

namespace ClipboardCanvas.ViewModels.UserControls.InAppNotifications
{
    public interface IInAppNotification
    {
        InAppNotificationControlViewModel ViewModel { get; set; }

        event EventHandler<InAppNotificationDismissedEventArgs> OnInAppNotificationDismissedEvent;

        Task Show(int milliseconds = 0);

        void Dismiss();
    }
}
