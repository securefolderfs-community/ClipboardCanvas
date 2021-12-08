using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Specialized;
using System.Collections;

using ClipboardCanvas.Helpers;
using ClipboardCanvas.ViewModels.ContextMenu;

namespace ClipboardCanvas.AttachedProperties
{
    public class FlyoutItemsSourceAttachedProperty : BaseObservableCollectionAttachedProperty<FlyoutItemsSourceAttachedProperty, BaseMenuFlyoutItemViewModel, MenuFlyout>
    {
        protected override void ObservableCollection_CollectionChanged(object sender, ObservableCollection<BaseMenuFlyoutItemViewModel> collection, NotifyCollectionChangedEventArgs e)
        {
            if (sender is not MenuFlyout menuFlyout || collection == null)
            {
                return;
            }

            UpdateMenuFlyoutItems(menuFlyout, collection, e.NewItems);
        }

        protected override void OnValueChanged(MenuFlyout sender, ObservableCollection<BaseMenuFlyoutItemViewModel> newValue)
        {
            base.OnValueChanged(sender, newValue);

            UpdateMenuFlyoutItems(sender, newValue, newValue);
        }

        private void UpdateMenuFlyoutItems(MenuFlyout menuFlyout, IList<BaseMenuFlyoutItemViewModel> collection, IList newItems)
        {
            if (newItems == null || newItems.Count == 0)
            {
                menuFlyout.Items.Clear();
                return;
            }

            IEnumerable<MenuFlyoutItemBase> flyoutItems = FlyoutHelpers.GetMenuFlyoutItems(collection);

            menuFlyout.Items.Clear();
            foreach (var item in flyoutItems)
            {
                menuFlyout.Items.Add(item);
            }
        }
    }
}
