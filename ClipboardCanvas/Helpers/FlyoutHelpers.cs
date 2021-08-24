using ClipboardCanvas.Extensions;
using ClipboardCanvas.ViewModels.ContextMenu;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace ClipboardCanvas.Helpers
{
    public class FlyoutHelpers
    {
        public static IList<MenuFlyoutItemBase> GetMenuFlyoutItems(IList<BaseMenuFlyoutItemViewModel> items)
        {
            List<MenuFlyoutItemBase> flyoutItems = new List<MenuFlyoutItemBase>();
            IList<BaseMenuFlyoutItemViewModel> filtered = FilterItems(items);

            foreach (var item in filtered)
            {
                flyoutItems.Add(GetMenuItem(item));
            }

            return flyoutItems;
        }

        public static MenuFlyoutItemBase GetMenuItem(BaseMenuFlyoutItemViewModel item)
        {
            if (item is MenuFlyoutSeparatorViewModel)
            {
                return new MenuFlyoutSeparator()
                {
                    Tag = item.Tag,
                    IsEnabled = item.IsEnabled
                };
            }
            else
            {
                return GetFlyoutItem(item);
            }
        }

        public static MenuFlyoutItemBase GetFlyoutItem(BaseMenuFlyoutItemViewModel item)
        {
            if (item is MenuFlyoutSubItemViewModel subItemViewModel)
            {
                MenuFlyoutSubItem flyoutSubItem = new MenuFlyoutSubItem()
                {
                    Text = subItemViewModel.Text,
                    Tag = subItemViewModel.Tag,
                    IsEnabled = subItemViewModel.IsEnabled,
                    Icon = new FontIcon() { Glyph = subItemViewModel.IconGlyph.PreventNull(string.Empty) }
                };

                foreach (var item2 in subItemViewModel.SubItems)
                {
                    flyoutSubItem.Items.Add(GetMenuItem(item2));
                }

                return flyoutSubItem;
            }
            else
            {
                return GetStandardItem(item);
            }
        }

        public static MenuFlyoutItemBase GetStandardItem(BaseMenuFlyoutItemViewModel item)
        {
            MenuFlyoutItemBase flyoutItem;

            if (item is MenuFlyoutToggleItemViewModel flyoutItemToggleViewModel)
            {
                flyoutItem = new ToggleMenuFlyoutItem()
                {
                    Command = flyoutItemToggleViewModel.Command,
                    CommandParameter = flyoutItemToggleViewModel.CommandParameter,
                    Text = flyoutItemToggleViewModel.Text,
                    Tag = flyoutItemToggleViewModel.Tag,
                    IsChecked = flyoutItemToggleViewModel.IsChecked
                };
            }
            else
            {
                MenuFlyoutItemViewModel flyoutItemViewModel = item as MenuFlyoutItemViewModel;

                flyoutItem = new MenuFlyoutItem()
                {
                    Command = flyoutItemViewModel.Command,
                    CommandParameter = flyoutItemViewModel.CommandParameter,
                    Text = flyoutItemViewModel.Text,
                    Tag = flyoutItemViewModel.Tag,
                    Icon = new FontIcon() { Glyph = flyoutItemViewModel.IconGlyph.PreventNull(string.Empty) }
                };
            }

            flyoutItem.IsEnabled = item.IsEnabled;

            return flyoutItem;
        }

        public static IList<BaseMenuFlyoutItemViewModel> FilterItems(IList<BaseMenuFlyoutItemViewModel> items)
        {
            List<BaseMenuFlyoutItemViewModel> filtered = new List<BaseMenuFlyoutItemViewModel>();

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].IsShown?.Invoke() ?? false)
                {
                    filtered.Add(items[i]);
                }
            }

            return filtered;
        }
    }
}
