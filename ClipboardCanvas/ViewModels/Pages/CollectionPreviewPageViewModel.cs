using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Linq;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using ClipboardCanvas.Enums;
using ClipboardCanvas.EventArguments.CollectionPreview;
using ClipboardCanvas.EventArguments.Collections;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls.Collections;
using ClipboardCanvas.Interfaces.Search;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Services;
using ClipboardCanvas.Serialization;

namespace ClipboardCanvas.ViewModels.Pages
{
    public class CollectionPreviewPageViewModel : ObservableObject, ICollectionPreviewPageModel, ISearchItems, IDisposable
    {
        #region Private Members

        private ICollectionModel _associatedCollectionModel => _view?.AssociatedCollectionModel;

        private ISearchControlModel _searchControlModel => _view?.SearchControlModel;

        private IUserSettingsService UserSettings { get; } = Ioc.Default.GetService<IUserSettingsService>();

        private readonly ICollectionPreviewPageView _view;

        private readonly DoubleClickWrapper _canvasPreviewItemDoubleClickWrapper;

        private bool _collectionInitializationFinishedEventHooked;

        private bool _suppressIndexReset;

        #endregion

        #region Public Properties

        private INavigationService NavigationService { get; } = Ioc.Default.GetService<INavigationService>();

        public RangeObservableCollection<CollectionPreviewItemViewModel> Items { get; private set; }

        private CollectionPreviewItemViewModel _SelectedItem;
        public CollectionPreviewItemViewModel SelectedItem
        {
            get => _SelectedItem;
            set
            {
                if (SetProperty(ref _SelectedItem, value))
                {
                    // Set selection property
                    foreach (var item in Items)
                    {
                        item.IsSelected = false;
                    }

                    if (!_suppressIndexReset)
                    {
                        _searchControlModel.ResetIndex();
                    }

                    if (_SelectedItem != null)
                    {
                        _SelectedItem.IsSelected = true;
                    }

                    OnCanvasPreviewSelectedItemChangedEvent?.Invoke(this, new CanvasPreviewSelectedItemChangedEventArgs(_SelectedItem));
                }
            }
        }

        private bool _CollectionLoadingIndicatorLoad;
        public bool CollectionLoadingIndicatorLoad
        {
            get => _CollectionLoadingIndicatorLoad;
            set => SetProperty(ref _CollectionLoadingIndicatorLoad, value);
        }

        private bool _NoItemsInfoTextLoad;
        public bool NoItemsInfoTextLoad
        {
            get => _NoItemsInfoTextLoad;
            set => SetProperty(ref _NoItemsInfoTextLoad, value);
        }

        private bool _IsSearchEnabled;
        public bool IsSearchEnabled
        {
            get => _IsSearchEnabled;
            set => SetProperty(ref _IsSearchEnabled, value);
        }

        private bool _SearchControlVisible;
        public bool SearchControlVisible
        {
            get => _SearchControlVisible;
            set => SetProperty(ref _SearchControlVisible, value);
        }

        public string SplitButtonMainOptionText
        {
            get
            {
                if (UserSettings.UseInfiniteCanvasAsDefault)
                {
                    return "Open new Infinite Canvas";
                }
                else
                {
                    return "Open new Canvas";
                }
            }
        }

        public bool SplitButtonShowInfiniteCanvasOption
        {
            get
            {
                if (UserSettings.UseInfiniteCanvasAsDefault)
                {
                    // Infinite Canvas is default, so show normal Canvas -> return false
                    return false;
                }
                else
                {
                    // ...else, true
                    return true;
                }
            }
        }

        public static CancellationTokenSource LoadCancellationToken = new CancellationTokenSource();

        #endregion

        #region Events

        public event EventHandler<CanvasPreviewSelectedItemChangedEventArgs> OnCanvasPreviewSelectedItemChangedEvent;

        #endregion

        #region Commands

        public ICommand SplitButtonDefaultOptionCommand { get; private set; }

        public ICommand OpenNewCanvasCommand { get; private set; }

        public ICommand OpenNewInfiniteCanvasCommand { get; private set; }

        public ICommand ItemClickCommand { get; private set; }

        public ICommand ShowOrHideSearchCommand { get; private set; }

        public ICommand ContainerChangingCommand { get; private set; }

        public ICommand DefaultKeyboardAcceleratorInvokedCommand { get; private set; }

        #endregion

        #region Constructor

        public CollectionPreviewPageViewModel(ICollectionPreviewPageView view)
        {
            this._view = view;

            HookEvents();

            this._canvasPreviewItemDoubleClickWrapper = new DoubleClickWrapper(
                ItemOpenRequested,
                TimeSpan.FromMilliseconds(Constants.Collections.DOUBLE_CLICK_DELAY_MILLISECONDS));
            this.Items = new RangeObservableCollection<CollectionPreviewItemViewModel>();

            _searchControlModel.SearchItems = this;
            _searchControlModel.IsKeyboardAcceleratorEnabled = true;

            // Create commands
            SplitButtonDefaultOptionCommand = new RelayCommand(SplitButtonDefaultOption);
            OpenNewCanvasCommand = new RelayCommand(OpenNewCanvas);
            OpenNewInfiniteCanvasCommand = new RelayCommand(OpenNewInfiniteCanvas);
            ItemClickCommand = new RelayCommand<ItemClickEventArgs>(ItemClick);
            ShowOrHideSearchCommand = new RelayCommand(ShowOrHideSearch);
            ContainerChangingCommand = new RelayCommand<ContainerContentChangingEventArgs>(ContainerContentChanging);
            DefaultKeyboardAcceleratorInvokedCommand = new RelayCommand<KeyboardAcceleratorInvokedEventArgs>(DefaultKeyboardAcceleratorInvoked);
        }

        #endregion

        #region Command Implementation

        private void SplitButtonDefaultOption()
        {
            if (UserSettings.UseInfiniteCanvasAsDefault)
            {
                OpenNewInfiniteCanvas();
            }
            else
            {
                OpenNewCanvas();
            }
        }

        private void OpenNewCanvas()
        {
            NavigationService.OpenNewCanvas(_associatedCollectionModel, canvasType: CanvasType.OneCanvas);
        }

        private void OpenNewInfiniteCanvas()
        {
            NavigationService.OpenNewCanvas(_associatedCollectionModel, canvasType: CanvasType.InfiniteCanvas);
        }

        private void ItemClick(ItemClickEventArgs e)
        {
            _canvasPreviewItemDoubleClickWrapper.Click();
        }

        private void ShowOrHideSearch()
        {
            if (SearchControlVisible)
            {
                HideSearch();
            }
            else
            {
                ShowSearch();
            }
        }

        private void ContainerContentChanging(ContainerContentChangingEventArgs e)
        {
            if (!e.InRecycleQueue)
            {
                if (e.Item is CollectionPreviewItemViewModel itemViewModel)
                {
                    e.RegisterUpdateCallback(3, async (s, e1) =>
                    {
                        await itemViewModel.RequestCanvasLoad();
                    });
                }
            }
            else
            {
                if (e.Item is CollectionPreviewItemViewModel itemViewModel)
                {
                    e.RegisterUpdateCallback(3, async (s, e1) =>
                    {
                        await itemViewModel.RequestCanvasUnload();
                    });
                }
            }
        }

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
                case (c: true, s: false, a: false, w: false, k: VirtualKey.F): // Show Search
                    {
                        ShowSearch();
                        break;
                    }

                case (c: false, s: false, a: false, w: false, k: VirtualKey.F3): // Find next
                    {
                        _searchControlModel.FindNext();
                        break;
                    }

                case (c: false, s: true, a: false, w: false, k: VirtualKey.F3): // Find previous
                    {
                        _searchControlModel.FindPrevious();
                        break;
                    }
            }
        }

        #endregion

        #region ISearchItems

        public int Count => Items.Count;

        public int CurrentIndex => Items.IndexOf(SelectedItem);

        public IEnumerable<ISearchItem> CompareItemsToPhrase(string phrase)
        {
            return Items.Where((item) => item.DisplayName.Contains(phrase, StringComparison.InvariantCultureIgnoreCase));
        }

        public int IndexOfItemInCollection(ISearchItem item)
        {
            return Items.IndexOf(item as CollectionPreviewItemViewModel);
        }

        public void SetSelectedIndex(int index)
        {
            _suppressIndexReset = true;
            SelectedItem = Items[index];
            _suppressIndexReset = false;

            _view?.ScrollIntoItemView(SelectedItem);
        }

        #endregion

        #region Public Helpers

        public void CheckSearchContext()
        {
            if (_associatedCollectionModel.SearchContext != null)
            {
                ShowSearch();
                _searchControlModel.RestoreSearchContext(_associatedCollectionModel.SearchContext);
            }
        }

        public async Task InitializeItems()
        {
            if (_associatedCollectionModel.IsCollectionInitializing)
            {
                // The collection is still initializing, show loading indicator
                CollectionLoadingIndicatorLoad = true;
                NoItemsInfoTextLoad = false;

                // Hook event when the collection initialization finishes to update the UI
                CollectionsControlViewModel.OnCollectionItemsInitializationFinishedEvent += CollectionsControlViewModel_OnCollectionItemsInitializationFinishedEvent;
                _collectionInitializationFinishedEventHooked = true;
            }
            else
            {
                List<Task<CollectionPreviewItemViewModel>> itemTaskDelegates = new List<Task<CollectionPreviewItemViewModel>>();
                foreach (var item in _associatedCollectionModel.CollectionItems)
                {
                    itemTaskDelegates.Add(CollectionPreviewItemViewModel.GetCollectionPreviewItemModel(_associatedCollectionModel, item));
                }

                CollectionLoadingIndicatorLoad = true;
                Items.AddRange(await Task.WhenAll(itemTaskDelegates));
                CollectionLoadingIndicatorLoad = false;

                bool itemsEmpty = Items.IsEmpty();
                NoItemsInfoTextLoad = itemsEmpty;
                IsSearchEnabled = !itemsEmpty;

                SetSelectedItemAfterInitialization();
            }
        }

        #endregion

        #region Private Helpers

        private void SetSelectedItemAfterInitialization()
        {
            if (_associatedCollectionModel.CurrentCollectionItemViewModel != null)
            {
                SelectedItem = Items.FirstOrDefault((item) => item.CollectionItemViewModel == _associatedCollectionModel.CurrentCollectionItemViewModel);

                _view?.ScrollIntoItemView(SelectedItem);
            }
        }

        private void ItemOpenRequested()
        {
            int indexOfSelectedItem = Items.IndexOf(SelectedItem);
            _view?.PrepareConnectedAnimation(indexOfSelectedItem);

            // Navigate to canvas and suppress transition since we use ConnectedAnimation
            NavigationService.OpenCanvasPage(_associatedCollectionModel, SelectedItem.CollectionItemViewModel);
        }

        private void ShowSearch()
        {
            SearchControlVisible = true;
            _searchControlModel.OnSearchShown();
        }

        private void HideSearch()
        {
            SearchControlVisible = false;
            _searchControlModel.OnSearchHidden();
        }

        #endregion

        #region Event Handlers

        private void UserSettings_OnSettingChangedEvent(object sender, SettingChangedEventArgs e)
        {
            // If the setting corresponding to new canvas button changed, update the split button
            if (e.settingName == nameof(UserSettings.UseInfiniteCanvasAsDefault))
            {
                OnPropertyChanged(nameof(SplitButtonMainOptionText));
                OnPropertyChanged(nameof(SplitButtonShowInfiniteCanvasOption));
            }
        }

        private void SearchControlModel_OnSearchCloseRequestedEvent(object sender, SearchCloseRequestedEventArgs e)
        {
            HideSearch();
        }

        private async void CollectionsControlViewModel_OnCollectionItemsInitializationFinishedEvent(object sender, CollectionItemsInitializationFinishedEventArgs e)
        {
            // Make sure the event was invoked for correct collection
            if (_associatedCollectionModel == e.baseCollectionViewModel)
            {
                // Initialize items again
                await InitializeItems();
            }
        }

        #endregion

        #region Event Hooks

        private void HookEvents()
        {
            UserSettings.OnSettingChangedEvent += UserSettings_OnSettingChangedEvent;

            if (_view.SearchControlModel != null)
            {
                _view.SearchControlModel.OnSearchCloseRequestedEvent += SearchControlModel_OnSearchCloseRequestedEvent;
            }
        }

        private void UnhookEvents()
        {
            UserSettings.OnSettingChangedEvent -= UserSettings_OnSettingChangedEvent;

            if (_view.SearchControlModel != null)
            {
                _view.SearchControlModel.OnSearchCloseRequestedEvent -= SearchControlModel_OnSearchCloseRequestedEvent;
            }

            if (_collectionInitializationFinishedEventHooked)
            {
                CollectionsControlViewModel.OnCollectionItemsInitializationFinishedEvent -= CollectionsControlViewModel_OnCollectionItemsInitializationFinishedEvent;
            }
        }

        #endregion

        #region IDisposable

        public async void Dispose()
        {
            await Task.Delay(1000); // Delay so item preview doesn't disappear instantly for connected animation

            Items?.DisposeClear();
            _canvasPreviewItemDoubleClickWrapper?.Dispose();
            UnhookEvents();

            if (SearchControlVisible)
            {
                _associatedCollectionModel.SearchContext = _searchControlModel.ConstructSearchContext();
            }
            else
            {
                _associatedCollectionModel.SearchContext = null;
            }
        }

        #endregion
    }
}
