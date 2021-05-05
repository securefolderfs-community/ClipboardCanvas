using ClipboardCanvas.Extensions;
using ClipboardCanvas.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class SuggestedActionsControlViewModel : ObservableObject, ISuggestedActionsControlModel
    {
        #region Public Properties

        public ObservableCollection<SuggestedActionsControlItemViewModel> Items { get; private set; }

        #endregion

        #region Commands

        public ICommand ItemClickCommand { get; private set; }

        public ICommand DefaultKeyboardAcceleratorInvokedCommand { get; private set; }

        #endregion

        #region Constructor

        public SuggestedActionsControlViewModel()
        {
            Items = new ObservableCollection<SuggestedActionsControlItemViewModel>();

            // Create commands
            ItemClickCommand = new RelayCommand<ItemClickEventArgs>(ItemClick);
            DefaultKeyboardAcceleratorInvokedCommand = new RelayCommand<KeyboardAcceleratorInvokedEventArgs>(DefaultKeyboardAcceleratorInvoked);
        }

        #endregion

        #region Command Implementation

        private void DefaultKeyboardAcceleratorInvoked(KeyboardAcceleratorInvokedEventArgs e)
        {
            bool ctrl = e.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Control);
            bool shift = e.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Shift);
            bool alt = e.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Menu);
            bool win = e.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Windows);
            VirtualKey vkey = e.KeyboardAccelerator.Key;
            uint uVkey = (uint)e.KeyboardAccelerator.Key;

            switch (c: ctrl, s: shift, a: alt, w: win, k: vkey)
            {
                case (c: true, s: false, a: false, w: false, k: VirtualKey.Number1):
                    {
                        Items.ElementAtOrDefault(0)?.ExecuteAction.Invoke();

                        break;
                    }

                case (c: true, s: false, a: false, w: false, k: VirtualKey.Number2):
                    {
                        Items.ElementAtOrDefault(1)?.ExecuteAction.Invoke();

                        break;
                    }

                case (c: true, s: false, a: false, w: false, k: VirtualKey.Number3):
                    {
                        Items.ElementAtOrDefault(2)?.ExecuteAction.Invoke();

                        break;
                    }
            }
        }

        private void ItemClick(ItemClickEventArgs e)
        {
            var clickedItem = e.ClickedItem as SuggestedActionsControlItemViewModel;
            clickedItem.ExecuteAction?.Invoke();
        }

        #endregion

        #region ISuggestedActionsControlModel

        public void SetActions(IEnumerable<SuggestedActionsControlItemViewModel> actions)
        {
            List<SuggestedActionsControlItemViewModel> itemsThatCollectionDoesntContain = Items.Where((item) => !actions.Contains(item)).ToList();

            itemsThatCollectionDoesntContain.ForEach((item) => RemoveAction(item));

            foreach (var item in actions)
            {
                AddAction(item);
            }
        }

        public void AddAction(SuggestedActionsControlItemViewModel action)
        {
            if (!Items.Contains(action))
            {
                Items.Add(action);
            }
        }

        public void RemoveAction(SuggestedActionsControlItemViewModel action)
        {
            int indexToRemove = Items.IndexOf(action);
            Items[indexToRemove].Dispose();
            Items.RemoveAt(indexToRemove);
        }

        public void RemoveActionAt(int index)
        {
            if (index < 0 || index > Items.Count || Items.IsEmpty())
            {
                return;
            }

            Items[index].Dispose();
            Items.RemoveAt(index);
        }

        public void RemoveAllActions()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Dispose();
                Items.RemoveAt(i);
            }
        }

        #endregion
    }
}
