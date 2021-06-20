using System;
using Windows.UI.Xaml.Controls;
using ClipboardCanvas.ViewModels.UserControls.InAppNotifications;
using ClipboardCanvas.EventArguments;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ClipboardCanvas.UserControls
{
    public sealed partial class InAppNotificationControl : UserControl, IInAppNotification
    {
        public InAppNotificationControlViewModel ViewModel
        {
            get => (InAppNotificationControlViewModel)DataContext;
            set => DataContext = value;
        }

        public event EventHandler<InAppNotificationDismissedEventArgs> OnInAppNotificationDismissedEvent;

        public InAppNotificationControl()
        {
            this.InitializeComponent();

            this.ViewModel = new InAppNotificationControlViewModel();
            this.ViewModel.OnInAppNotificationDismissedEvent += ViewModel_OnInAppNotificationDismissedEvent;
        }

        public void Show(int miliseconds = 0)
        {
            ViewModel?.ShowNotification(miliseconds);
        }

        public void Dismiss()
        {
            ViewModel?.DismissNotification();
        }

        private void ViewModel_OnInAppNotificationDismissedEvent(object sender, InAppNotificationDismissedEventArgs e)
        {
            this.ViewModel.OnInAppNotificationDismissedEvent -= ViewModel_OnInAppNotificationDismissedEvent;
            OnInAppNotificationDismissedEvent?.Invoke(sender, e);
        }
    }
}
