using System.Threading;

using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.Services
{
    public interface INotificationService
    {
        CancellationToken PushAutopastePasteStartedNotification();

        void PushAutopastePasteFinishedNotification();

        void PushAutopastePasteFailedNotification(SafeWrapperResult result);
    }
}
