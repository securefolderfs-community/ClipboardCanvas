using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Threading;
using Windows.UI.Notifications;

using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        public CancellationToken PushAutopastePasteStartedNotification()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            ToastContent toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "AutopasteWorking".GetLocalized()
                            },
                            new AdaptiveText()
                            {
                                Text = "AutopasteContentBeingPasted".GetLocalized()
                            }
                        }
                    }
                }
            };

            ToastNotification toastNotificationNative = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toastNotificationNative);

            return cancellationTokenSource.Token;
        }

        public void PushAutopastePasteFinishedNotification()
        {
            ToastContent toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "AutopasteFinishedWorking".GetLocalized()
                            },
                            new AdaptiveText()
                            {
                                Text = "AutopasteContentPasted".GetLocalized()
                            }
                        }
                    }
                }
            };

            ToastNotification toastNotificationNative = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toastNotificationNative);
        }

        public void PushAutopastePasteFailedNotification(SafeWrapperResult result)
        {
            ToastContent toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "AutopastePastingFailed".GetLocalized()
                            },
                            new AdaptiveText()
                            {
                                Text = string.Format("AutopasteContentPastingFailed".GetLocalized(), result?.ErrorCode ?? OperationErrorCode.UnknownFailed)
                            }
                        }
                    }
                }
            };

            ToastNotification toastNotificationNative = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toastNotificationNative);
        }
    }
}
