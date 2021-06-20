using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Xaml;
using ClipboardCanvas.DataModels;

namespace ClipboardCanvas.AttachedProperties
{
    public class InAppNotificationShowHideControlAttachedProperty : BaseAttachedProperty<InAppNotificationShowHideControlAttachedProperty, InAppNotificationDisplayOptionsDataModel, InAppNotification>
    {
        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not InAppNotification notification || e.NewValue is not InAppNotificationDisplayOptionsDataModel value)
            {
                return;
            }

            if (value.show)
            {
                notification.Show(value.miliseconds);
            }
            else
            {
                notification.Dismiss();
            }
        }
    }
}
