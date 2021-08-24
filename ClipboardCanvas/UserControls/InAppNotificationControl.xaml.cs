using System;
using Windows.UI.Xaml.Controls;
using ClipboardCanvas.ViewModels.UserControls.InAppNotifications;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Enums;
using System.Threading.Tasks;

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

            this.ViewModel = new InAppNotificationControlViewModel(InAppNotificationButtonType.OkButton);
            this.ViewModel.OnInAppNotificationDismissedEvent += ViewModel_OnInAppNotificationDismissedEvent;
        }

        public void Show(int milliseconds = 0)
        {
            ViewModel.Show(milliseconds);
        }

        public void Dismiss()
        {
            ViewModel.Dismiss();
        }

        private void ViewModel_OnInAppNotificationDismissedEvent(object sender, InAppNotificationDismissedEventArgs e)
        {
            this.ViewModel.OnInAppNotificationDismissedEvent -= ViewModel_OnInAppNotificationDismissedEvent;
            OnInAppNotificationDismissedEvent?.Invoke(sender, e);
        }
    }
}
