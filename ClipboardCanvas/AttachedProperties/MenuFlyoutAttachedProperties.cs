using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

using ClipboardCanvas.Helpers;
using ClipboardCanvas.ViewModels.ContextMenu;

namespace ClipboardCanvas.AttachedProperties
{
    public class FlyoutItemsSourceAttachedProperty : BaseAttachedProperty<FlyoutItemsSourceAttachedProperty, List<BaseMenuFlyoutItemViewModel>, MenuFlyout>
    {
        public override void OnValueChanged(MenuFlyout sender, List<BaseMenuFlyoutItemViewModel> newValue)
        {
            IList<MenuFlyoutItemBase> flyoutItems = FlyoutHelpers.GetMenuFlyoutItems(newValue);

            sender.Items.Clear();
            foreach (var item in flyoutItems)
            {
                sender.Items.Add(item);
            }
        }
    }
}
