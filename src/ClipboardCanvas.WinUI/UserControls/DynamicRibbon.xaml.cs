using ClipboardCanvas.Sdk.ViewModels.Controls.Menu;
using ClipboardCanvas.WinUI.Imaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OwlCore.Storage;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.UserControls
{
    public sealed partial class DynamicRibbon : UserControl
    {
        public DynamicRibbon()
        {
            InitializeComponent();
        }

        private static void ParseCommands(IList<MenuItemViewModel>? items, IList<ICommandBarElement> destination)
        {
            destination.Clear();
            if (items is null)
                return;

            foreach (var item in items)
            {
                switch (item)
                {
                    case MenuToggleViewModel toggle:
                    {
                        var element = new AppBarToggleButton();
                        element.Icon = new FontIcon() { Glyph = (toggle.Icon as IconImage)?.IconGlyph };
                        element.Label = toggle.Name;
                        element.Command = toggle.Command;

                        destination.Add(element);
                        break;
                    }

                    case MenuActionViewModel action:
                    {
                        var element = new AppBarButton();
                        element.Icon = new FontIcon() { Glyph = (action.Icon as IconImage)?.IconGlyph };
                        element.Label = action.Name;
                        element.Command = action.Command;

                        destination.Add(element);
                        break;
                    }
                }
            }
        }

        private void PrimaryItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ParseCommands(PrimaryActions, Commands.PrimaryCommands);
        }

        private void SecondaryItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ParseCommands(SecondaryActions, Commands.SecondaryCommands);
        }

        private void MenuFlyout_Opening(object sender, object e)
        {
            if (sender is not MenuFlyout menuFlyout)
                return;

            if (RenameCommand is null && CopyPathCommand is null && ShowInExplorerCommand is null)
                menuFlyout.Hide();
        }

        public ICommand? RenameCommand
        {
            get => (ICommand?)GetValue(RenameCommandProperty);
            set => SetValue(RenameCommandProperty, value);
        }
        public static readonly DependencyProperty RenameCommandProperty =
            DependencyProperty.Register(nameof(RenameCommand), typeof(IStorableChild), typeof(DynamicRibbon), new PropertyMetadata(null));

        public ICommand? CopyPathCommand
        {
            get => (ICommand?)GetValue(CopyPathCommandProperty);
            set => SetValue(CopyPathCommandProperty, value);
        }
        public static readonly DependencyProperty CopyPathCommandProperty =
            DependencyProperty.Register(nameof(CopyPathCommand), typeof(IStorableChild), typeof(DynamicRibbon), new PropertyMetadata(null));

        public ICommand? ShowInExplorerCommand
        {
            get => (ICommand?)GetValue(ShowInExplorerCommandProperty);
            set => SetValue(ShowInExplorerCommandProperty, value);
        }
        public static readonly DependencyProperty ShowInExplorerCommandProperty =
            DependencyProperty.Register(nameof(ShowInExplorerCommand), typeof(IStorableChild), typeof(DynamicRibbon), new PropertyMetadata(null));

        public string? ToolBarTitle
        {
            get => (string?)GetValue(ToolBarTitleProperty);
            set => SetValue(ToolBarTitleProperty, value);
        }
        public static readonly DependencyProperty ToolBarTitleProperty =
            DependencyProperty.Register(nameof(ToolBarTitle), typeof(string), typeof(DynamicRibbon), new PropertyMetadata(null));

        public IList<MenuItemViewModel>? PrimaryActions
        {
            get => (IList<MenuItemViewModel>?)GetValue(PrimaryActionsProperty);
            set => SetValue(PrimaryActionsProperty, value);
        }
        public static readonly DependencyProperty PrimaryActionsProperty =
            DependencyProperty.Register(nameof(PrimaryActions), typeof(IList<MenuItemViewModel>), typeof(DynamicRibbon), new PropertyMetadata(null, (s, e) =>
            {
                if (s is not DynamicRibbon ribbon)
                    return;

                if (e.OldValue is INotifyCollectionChanged notifiableOld)
                    notifiableOld.CollectionChanged -= ribbon.PrimaryItems_CollectionChanged;

                if (e.NewValue is INotifyCollectionChanged notifiableNew)
                    notifiableNew.CollectionChanged += ribbon.PrimaryItems_CollectionChanged;

                ParseCommands((IList<MenuItemViewModel>?)e.NewValue, ribbon.Commands.PrimaryCommands);
            }));

        public IList<MenuItemViewModel>? SecondaryActions
        {
            get => (IList<MenuItemViewModel>?)GetValue(SecondaryActionsProperty);
            set => SetValue(SecondaryActionsProperty, value);
        }
        public static readonly DependencyProperty SecondaryActionsProperty =
            DependencyProperty.Register(nameof(SecondaryActions), typeof(IList<MenuItemViewModel>), typeof(DynamicRibbon), new PropertyMetadata(null, (s, e) =>
            {
                if (s is not DynamicRibbon ribbon)
                    return;

                if (e.OldValue is INotifyCollectionChanged notifiableOld)
                    notifiableOld.CollectionChanged -= ribbon.PrimaryItems_CollectionChanged;

                if (e.NewValue is INotifyCollectionChanged notifiableNew)
                    notifiableNew.CollectionChanged += ribbon.PrimaryItems_CollectionChanged;

                ParseCommands((IList<MenuItemViewModel>?)e.NewValue, ribbon.Commands.SecondaryCommands);
            }));
    }
}
