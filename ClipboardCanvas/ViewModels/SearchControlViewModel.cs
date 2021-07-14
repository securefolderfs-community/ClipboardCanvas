using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Input;
using Windows.System;
using System.Windows.Input;

using ClipboardCanvas.Extensions;
using ClipboardCanvas.Interfaces.Search;
using ClipboardCanvas.Models;
using ClipboardCanvas.EventArguments;

namespace ClipboardCanvas.ViewModels
{
    public class SearchControlViewModel : ObservableObject, ISearchControlModel
    {
        #region Private Members

        private IEnumerable<ISearchItem> _searchMatchingItems;

        private int _indexInSearch = 0;

        #endregion

        #region Public Properties

        public ISearchItems SearchItems { get; set; }

        private bool _IsKeyboardAcceleratorEnabled;
        public bool IsKeyboardAcceleratorEnabled
        {
            get => _IsKeyboardAcceleratorEnabled;
            set => SetProperty(ref _IsKeyboardAcceleratorEnabled, value);
        }

        private string _SearchBoxText;
        public string SearchBoxText
        {
            get => _SearchBoxText;
            set
            {
                if (SetProperty(ref _SearchBoxText, value))
                {
                    Search(_SearchBoxText);
                }
            }
        }

        private bool _IsNextPreviousEnabled;
        public bool IsNextPreviousEnabled
        {
            get => _IsNextPreviousEnabled;
            set => SetProperty(ref _IsNextPreviousEnabled, value);
        }

        private bool _SearchBoxFocus;
        public bool SearchBoxFocus
        {
            get => _SearchBoxFocus;
            set => SetProperty(ref _SearchBoxFocus, value);
        }

        #endregion

        #region Events

        public event EventHandler<SearchCloseRequestedEventArgs> OnSearchCloseRequestedEvent;

        #endregion

        #region Commands

        public ICommand FindNextCommand { get; private set; }

        public ICommand FindPreviousCommand { get; private set; }

        public ICommand HideSearchCommand { get; private set; }

        public ICommand DefaultKeyboardAcceleratorInvokedCommand { get; private set; }

        #endregion

        #region Constructor

        public SearchControlViewModel()
        {
            // Create Commands
            FindNextCommand = new RelayCommand(FindNext);
            FindPreviousCommand = new RelayCommand(FindPrevious);
            HideSearchCommand = new RelayCommand(HideSearch);
            DefaultKeyboardAcceleratorInvokedCommand = new RelayCommand<KeyboardAcceleratorInvokedEventArgs>(DefaultKeyboardAcceleratorInvoked);
        }

        #endregion

        #region Command Implementation

        private void DefaultKeyboardAcceleratorInvoked(KeyboardAcceleratorInvokedEventArgs e)
        {
            e.Handled = true;
            bool ctrl = e.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Control);
            bool shift = e.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Shift);
            bool alt = e.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Menu);
            bool win = e.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Windows);
            VirtualKey vkey = e.KeyboardAccelerator.Key;
            uint uVkey = (uint)e.KeyboardAccelerator.Key;

            switch (c: ctrl, s: shift, a: alt, w: win, k: vkey)
            {
                case (c: false, s: false, a: false, w: false, k: VirtualKey.Escape): // Close Search
                    {
                        HideSearch();
                        break;
                    }
            }
        }

        #endregion

        #region ISearchControlModel

        public void FindNext()
        {
            _indexInSearch++;

            if (_indexInSearch >= _searchMatchingItems.Count())
            {
                _indexInSearch = 0;
                UpdateItemSelectionOnForward(true);
            }
            else
            {
                UpdateItemSelectionOnForward(false);
            }
        }

        public void FindPrevious()
        {
            _indexInSearch--;

            if (_indexInSearch < 0)
            {
                _indexInSearch = _searchMatchingItems.Count() - 1;
                UpdateItemSelectionOnBack(true);
            }
            else
            {
                UpdateItemSelectionOnBack(false);
            }
        }

        public void OnSearchShown()
        {
            SearchBoxFocus = true;
            SearchBoxFocus = false;
        }

        public void OnSearchHidden()
        {
            SearchBoxText = string.Empty;
            _indexInSearch = 0;
        }

        public void ResetIndex()
        {
            _indexInSearch = 0;
        }

        #endregion

        #region Private Helpers

        private void Search(string phrase)
        {
            if (!_searchMatchingItems.IsEmpty())
            {
                // Unselect previous items
                foreach (var item in _searchMatchingItems)
                {
                    item.IsHighlighted = false;
                }
            }

            // Don't search empty phrases
            if (string.IsNullOrEmpty(phrase))
            {
                IsNextPreviousEnabled = false;
                return;
            }

            // Get matching items
            _searchMatchingItems = SearchItems.CompareItemsToPhrase(phrase);

            if (_searchMatchingItems.IsEmpty())
            {
                IsNextPreviousEnabled = false;
            }
            else
            {
                // Select matching items
                foreach (var item in _searchMatchingItems)
                {
                    item.IsHighlighted = true;
                }

                IsNextPreviousEnabled = true;
            }
        }

        private void UpdateItemSelectionOnForward(bool ignoreCurrentIndex)
        {
            int indexToSelect;
            int currentSelectedIndex = SearchItems.CurrentIndex;
            int searchIndexInCollection = SearchItems.IndexOfItemInCollection(_searchMatchingItems.ElementAt(_indexInSearch));

            if (!ignoreCurrentIndex && currentSelectedIndex >= searchIndexInCollection)
            {
                while (currentSelectedIndex >= searchIndexInCollection)
                {
                    _indexInSearch++;

                    if (FitIndexIfOutOfBounds())
                    {
                        // Reached end of the sequence
                        searchIndexInCollection = SearchItems.IndexOfItemInCollection(_searchMatchingItems.ElementAt(_indexInSearch));
                        break;
                    }

                    searchIndexInCollection = SearchItems.IndexOfItemInCollection(_searchMatchingItems.ElementAt(_indexInSearch));
                }
            }

            indexToSelect = searchIndexInCollection;

            SearchItems.SetSelectedIndex(indexToSelect);
        }

        private void UpdateItemSelectionOnBack(bool ignoreCurrentIndex)
        {
            int indexToSelect;
            int currentSelectedIndex = SearchItems.CurrentIndex;
            int searchIndexInCollection = SearchItems.IndexOfItemInCollection(_searchMatchingItems.ElementAt(_indexInSearch));

            if (!ignoreCurrentIndex && currentSelectedIndex <= searchIndexInCollection)
            {
                while (currentSelectedIndex <= searchIndexInCollection)
                {
                    _indexInSearch--;

                    if (FitIndexIfOutOfBounds())
                    {
                        // Reached end of the sequence
                        searchIndexInCollection = SearchItems.IndexOfItemInCollection(_searchMatchingItems.ElementAt(_indexInSearch));
                        break;
                    }

                    searchIndexInCollection = SearchItems.IndexOfItemInCollection(_searchMatchingItems.ElementAt(_indexInSearch));
                }
            }

            indexToSelect = searchIndexInCollection;

            SearchItems.SetSelectedIndex(indexToSelect);
        }

        private bool FitIndexIfOutOfBounds()
        {
            if (_indexInSearch > _searchMatchingItems.Count())
            {
                _indexInSearch = 0;
                return true;
            }
            else if (_indexInSearch < 0)
            {
                _indexInSearch = _searchMatchingItems.Count() - 1;
                return true;
            }

            return false;
        }

        private void HideSearch()
        {
            OnSearchHidden();

            OnSearchCloseRequestedEvent?.Invoke(this, new SearchCloseRequestedEventArgs());
        }

        #endregion
    }
}
