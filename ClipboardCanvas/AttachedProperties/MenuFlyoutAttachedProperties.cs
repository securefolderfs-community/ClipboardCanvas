using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.ViewModels.ContextMenu;

namespace ClipboardCanvas.AttachedProperties
{
    public class FlyoutItemsSourceAttachedProperty : BaseAttachedProperty<FlyoutItemsSourceAttachedProperty, List<BaseMenuFlyoutItemViewModel>, MenuFlyout>
    {
        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            MenuFlyout flyout = sender as MenuFlyout;
            IList<BaseMenuFlyoutItemViewModel> items = e.NewValue as List<BaseMenuFlyoutItemViewModel>;
            IList<MenuFlyoutItemBase> flyoutItems = FlyoutHelpers.GetMenuFlyoutItems(items);

            flyout.Items.Clear();
            foreach (var item in flyoutItems)
            {
                flyout.Items.Add(item);
            }
        }
    }
}
