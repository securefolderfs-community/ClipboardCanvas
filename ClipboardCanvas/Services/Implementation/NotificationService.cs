using CommunityToolkit.WinUI.Notifications;
using System.Threading;
using Windows.UI.Notifications;

using ClipboardCanvas.Enums;
using ClipboardCanvas.GlobalizationExtensions;
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
                                Text = "AutopasteWorking".GetLocalized2()
                            },
                            new AdaptiveText()
                            {
                                Text = "AutopasteContentBeingPasted".GetLocalized2()
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
                                Text = "AutopasteFinishedWorking".GetLocalized2()
                            },
                            new AdaptiveText()
                            {
                                Text = "AutopasteContentPasted".GetLocalized2()
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
                                Text = "AutopastePastingFailed".GetLocalized2()
                            },
                            new AdaptiveText()
                            {
                                Text = string.Format("AutopasteContentPastingFailed".GetLocalized2(), result?.ErrorCode ?? OperationErrorCode.UnknownFailed)
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
