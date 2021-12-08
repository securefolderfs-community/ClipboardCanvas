using ClipboardCanvas.Enums;

namespace ClipboardCanvas.EventArguments
{
    public class InAppNotificationDismissedEventArgs
    {
        public readonly InAppNotificationButtonType notificationResult;

        public InAppNotificationDismissedEventArgs(InAppNotificationButtonType notificationResult)
        {
            this.notificationResult = notificationResult;
        }
    }
}
