using Microsoft.Toolkit.Uwp.UI.Controls;

namespace ClipboardCanvas.AttachedProperties
{
    public class InAppNotificationShowHideControlAttachedProperty : BaseAttachedProperty<InAppNotificationShowHideControlAttachedProperty, bool, InAppNotification>
    {
        protected override void OnValueChanged(InAppNotification sender, bool newValue)
        {
            if (newValue)
            {
                sender.Show();
            }
            else
            {
                sender.Dismiss();
            }
        }
    }
}
