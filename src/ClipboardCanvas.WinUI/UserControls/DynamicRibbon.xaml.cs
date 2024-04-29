using ClipboardCanvas.Sdk.ViewModels.Controls.Ribbon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

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

        private static void ParseCommands(IList<RibbonItemViewModel>? items, IList<ICommandBarElement> destination)
        {
            if (items is null)
            {
                destination.Clear();
                return;
            }


        }

        private void PrimaryItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ParseCommands(PrimaryItems, Commands.PrimaryCommands);
        }

        private void SecondaryItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ParseCommands(SecondaryItems, Commands.SecondaryCommands);
        }

        public string? ToolBarTitle
        {
            get => (string?)GetValue(ToolBarTitleProperty);
            set => SetValue(ToolBarTitleProperty, value);
        }
        public static readonly DependencyProperty ToolBarTitleProperty =
            DependencyProperty.Register(nameof(ToolBarTitle), typeof(string), typeof(DynamicRibbon), new PropertyMetadata(null));

        public IList<RibbonItemViewModel>? PrimaryItems
        {
            get => (IList<RibbonItemViewModel>?)GetValue(PrimaryItemsProperty);
            set => SetValue(PrimaryItemsProperty, value);
        }
        public static readonly DependencyProperty PrimaryItemsProperty =
            DependencyProperty.Register(nameof(PrimaryItems), typeof(IList<RibbonItemViewModel>), typeof(DynamicRibbon), new PropertyMetadata(null, (s, e) =>
            {
                if (s is not DynamicRibbon ribbon)
                    return;

                if (e.OldValue is INotifyCollectionChanged notifiableOld)
                    notifiableOld.CollectionChanged -= ribbon.PrimaryItems_CollectionChanged;

                if (e.NewValue is INotifyCollectionChanged notifiableNew)
                    notifiableNew.CollectionChanged += ribbon.PrimaryItems_CollectionChanged;

                ParseCommands((IList<RibbonItemViewModel>?)e.NewValue, ribbon.Commands.PrimaryCommands);
            }));

        public IList<RibbonItemViewModel>? SecondaryItems
        {
            get => (IList<RibbonItemViewModel>?)GetValue(SecondaryItemsProperty);
            set => SetValue(SecondaryItemsProperty, value);
        }
        public static readonly DependencyProperty SecondaryItemsProperty =
            DependencyProperty.Register(nameof(SecondaryItems), typeof(IList<RibbonItemViewModel>), typeof(DynamicRibbon), new PropertyMetadata(null, (s, e) =>
            {
                if (s is not DynamicRibbon ribbon)
                    return;

                if (e.OldValue is INotifyCollectionChanged notifiableOld)
                    notifiableOld.CollectionChanged -= ribbon.PrimaryItems_CollectionChanged;

                if (e.NewValue is INotifyCollectionChanged notifiableNew)
                    notifiableNew.CollectionChanged += ribbon.PrimaryItems_CollectionChanged;

                ParseCommands((IList<RibbonItemViewModel>?)e.NewValue, ribbon.Commands.SecondaryCommands);
            }));
    }
}
