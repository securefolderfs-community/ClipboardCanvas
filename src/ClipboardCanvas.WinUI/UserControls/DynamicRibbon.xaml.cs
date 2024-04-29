using ClipboardCanvas.Sdk.ViewModels.Controls.Ribbon;
using ClipboardCanvas.WinUI.Imaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
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

        private static void ParseCommands(IList<ActionViewModel>? items, IList<ICommandBarElement> destination)
        {
            destination.Clear();
            if (items is null)
                return;

            foreach (var item in items)
            {
                switch (item)
                {
                    case ToggleViewModel toggle:
                    {
                        var element = new AppBarToggleButton();
                        element.Icon = new FontIcon() { Glyph = (toggle.Icon as IconImage)?.IconGlyph };
                        element.Label = toggle.Name;
                        element.Command = toggle.Command;

                        destination.Add(element);
                        break;
                    }

                    case ActionViewModel action:
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

        public string? ToolBarTitle
        {
            get => (string?)GetValue(ToolBarTitleProperty);
            set => SetValue(ToolBarTitleProperty, value);
        }
        public static readonly DependencyProperty ToolBarTitleProperty =
            DependencyProperty.Register(nameof(ToolBarTitle), typeof(string), typeof(DynamicRibbon), new PropertyMetadata(null));

        public IList<ActionViewModel>? PrimaryActions
        {
            get => (IList<ActionViewModel>?)GetValue(PrimaryActionsProperty);
            set => SetValue(PrimaryActionsProperty, value);
        }
        public static readonly DependencyProperty PrimaryActionsProperty =
            DependencyProperty.Register(nameof(PrimaryActions), typeof(IList<ActionViewModel>), typeof(DynamicRibbon), new PropertyMetadata(null, (s, e) =>
            {
                if (s is not DynamicRibbon ribbon)
                    return;

                if (e.OldValue is INotifyCollectionChanged notifiableOld)
                    notifiableOld.CollectionChanged -= ribbon.PrimaryItems_CollectionChanged;

                if (e.NewValue is INotifyCollectionChanged notifiableNew)
                    notifiableNew.CollectionChanged += ribbon.PrimaryItems_CollectionChanged;

                ParseCommands((IList<ActionViewModel>?)e.NewValue, ribbon.Commands.PrimaryCommands);
            }));

        public IList<ActionViewModel>? SecondaryActions
        {
            get => (IList<ActionViewModel>?)GetValue(SecondaryActionsProperty);
            set => SetValue(SecondaryActionsProperty, value);
        }
        public static readonly DependencyProperty SecondaryActionsProperty =
            DependencyProperty.Register(nameof(SecondaryActions), typeof(IList<ActionViewModel>), typeof(DynamicRibbon), new PropertyMetadata(null, (s, e) =>
            {
                if (s is not DynamicRibbon ribbon)
                    return;

                if (e.OldValue is INotifyCollectionChanged notifiableOld)
                    notifiableOld.CollectionChanged -= ribbon.PrimaryItems_CollectionChanged;

                if (e.NewValue is INotifyCollectionChanged notifiableNew)
                    notifiableNew.CollectionChanged += ribbon.PrimaryItems_CollectionChanged;

                ParseCommands((IList<ActionViewModel>?)e.NewValue, ribbon.Commands.SecondaryCommands);
            }));
    }
}
