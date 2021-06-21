using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Xaml;

namespace ClipboardCanvas.AttachedProperties
{
    public class InAppNotificationShowHideControlAttachedProperty : BaseAttachedProperty<InAppNotificationShowHideControlAttachedProperty, bool, InAppNotification>
    {
        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not InAppNotification notification || e.NewValue is not bool value)
            {
                return;
            }

            if (value)
            {
                notification.Show();
            }
            else
            {
                notification.Dismiss();
            }
        }
    }
}
