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
using Windows.ApplicationModel.Core;
using Microsoft.Toolkit.Uwp;
using System.Threading.Tasks;

using ClipboardCanvas.Enums;
using ClipboardCanvas.EventArguments.CollectionPreview;
using ClipboardCanvas.EventArguments.Collections;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls.Collections;
using ClipboardCanvas.Interfaces.Search;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Services;
using ClipboardCanvas.Serialization;
using ClipboardCanvas.DataModels.Navigation;

namespace ClipboardCanvas.ViewModels.Pages
{
    public class CollectionPreviewPageViewModel : ObservableObject, ICollectionPreviewPageModel, ISearchItems, IDisposable
    {
        #region Private Members

        private readonly ICollectionPreviewPageView _view;

        private bool _collectionInitializationFinishedEventHooked;

        private bool _suppressIndexReset;

        #endregion

        #region Properties

        private IUserSettingsService UserSettings { get; } = Ioc.Default.GetService<IUserSettingsService>();

        private INavigationService NavigationService { get; } = Ioc.Default.GetService<INavigationService>();

        private ICollectionModel AssociatedCollectionModel => _view?.AssociatedCollectionModel;

        private ISearchControlModel SearchControlModel => _view?.SearchControlModel;

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
                        SearchControlModel.ResetIndex();
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
            get => UserSettings.UseInfiniteCanvasAsDefault ? "Open new Infinite Canvas" : "Open new Canvas";
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

        #endregion

        #region Events

        public event EventHandler<CanvasPreviewSelectedItemChangedEventArgs> OnCanvasPreviewSelectedItemChangedEvent;

        #endregion

        #region Commands

        public ICommand SplitButtonDefaultOptionCommand { get; private set; }

        public ICommand OpenNewCanvasCommand { get; private set; }

        public ICommand OpenNewInfiniteCanvasCommand { get; private set; }

        public ICommand ShowOrHideSearchCommand { get; private set; }

        public ICommand ContainerChangingCommand { get; private set; }

        public ICommand DefaultKeyboardAcceleratorInvokedCommand { get; private set; }

        #endregion

        #region Constructor

        public CollectionPreviewPageViewModel(ICollectionPreviewPageView view)
        {
            this._view = view;

            HookEvents();

            this.Items = new RangeObservableCollection<CollectionPreviewItemViewModel>();
            SearchControlModel.SearchItems = this;
            SearchControlModel.IsKeyboardAcceleratorEnabled = true;

            // Create commands
            SplitButtonDefaultOptionCommand = new RelayCommand(SplitButtonDefaultOption);
            OpenNewCanvasCommand = new RelayCommand(OpenNewCanvas);
            OpenNewInfiniteCanvasCommand = new RelayCommand(OpenNewInfiniteCanvas);
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
            NavigationService.OpenNewCanvas(AssociatedCollectionModel, canvasType: CanvasType.OneCanvas);
        }

        private void OpenNewInfiniteCanvas()
        {
            NavigationService.OpenNewCanvas(AssociatedCollectionModel, canvasType: CanvasType.InfiniteCanvas);
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
                        SearchControlModel.FindNext();
                        break;
                    }

                case (c: false, s: true, a: false, w: false, k: VirtualKey.F3): // Find previous
                    {
                        SearchControlModel.FindPrevious();
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

            _view?.ScrollItemToView(SelectedItem);
        }

        #endregion

        #region Public Helpers

        public void OpenItem(CollectionPreviewItemViewModel itemToOpen)
        {
            int indexOfSelectedItem = Items.IndexOf(itemToOpen);
            _view.PrepareConnectedAnimation(indexOfSelectedItem);

            // Navigate to canvas and suppress transition since we use ConnectedAnimation
            NavigationService.OpenCanvasPage(
                AssociatedCollectionModel,
                itemToOpen.CollectionItemViewModel,
                new CanvasPageNavigationParameterModel(AssociatedCollectionModel, AssociatedCollectionModel.AssociatedCanvasType)
                {
                    CollectionPreviewIDisposable = this
                },
                NavigationTransitionType.Suppress);
        }

        public void CheckSearchContext()
        {
            if (AssociatedCollectionModel.SearchContext != null)
            {
                ShowSearch();
                SearchControlModel.RestoreSearchContext(AssociatedCollectionModel.SearchContext);
            }
        }

        public async Task Initialize()
        {
            _view.AssociatedCollectionModel.OnCollectionItemAddedEvent += AssociatedCollectionModel_OnCollectionItemAddedEvent;
            _view.AssociatedCollectionModel.OnCollectionItemRemovedEvent += AssociatedCollectionModel_OnCollectionItemRemovedEvent;
            _view.AssociatedCollectionModel.OnCollectionItemRenamedEvent += AssociatedCollectionModel_OnCollectionItemRenamedEvent;

            await InitializeItems();
        }

        #endregion

        #region Private Helpers

        private async Task InitializeItems()
        {
            if (AssociatedCollectionModel.IsCollectionInitializing)
            {
                // The collection is still initializing, show loading indicator
                CollectionLoadingIndicatorLoad = true;
                NoItemsInfoTextLoad = false;

                // Hook event when the collection initialization finishes to update the UI
                CollectionsWidgetViewModel.OnCollectionItemsInitializationFinishedEvent += CollectionsControlViewModel_OnCollectionItemsInitializationFinishedEvent;
                _collectionInitializationFinishedEventHooked = true;
            }
            else
            {
                List<Task<CollectionPreviewItemViewModel>> itemTaskDelegates = new List<Task<CollectionPreviewItemViewModel>>();
                foreach (var item in AssociatedCollectionModel.CollectionItems)
                {
                    itemTaskDelegates.Add(CollectionPreviewItemViewModel.GetCollectionPreviewItemModel(AssociatedCollectionModel, item));
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

        private void SetSelectedItemAfterInitialization()
        {
            if (AssociatedCollectionModel.CurrentCollectionItemViewModel != null)
            {
                SelectedItem = Items.FirstOrDefault((item) => item.CollectionItemViewModel == AssociatedCollectionModel.CurrentCollectionItemViewModel);

                _view?.ScrollToItemOnInitialization(SelectedItem);
            }
            else
            {
                // If _canvasItemToScrollTo was set -- it will scroll to it, otherwise it'll do nothing
                _view?.ScrollToItemOnInitialization(null);
            }
        }

        private void ShowSearch()
        {
            SearchControlVisible = true;
            SearchControlModel.OnSearchShown();
        }

        private void HideSearch()
        {
            SearchControlVisible = false;
            SearchControlModel.OnSearchHidden();
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

        private async void AssociatedCollectionModel_OnCollectionItemAddedEvent(object sender, CollectionItemAddedEventArgs e)
        {
            if (AssociatedCollectionModel.IsCollectionInitializing)
            {
                // Don't add items when the collection is initializing
                return;
            }

            await CoreApplication.MainView.DispatcherQueue.EnqueueAsync(async () =>
            {
                var itemToAdd = await CollectionPreviewItemViewModel.GetCollectionPreviewItemModel(AssociatedCollectionModel, e.itemChanged);
                Items.Add(itemToAdd);
            });
        }

        private async void AssociatedCollectionModel_OnCollectionItemRemovedEvent(object sender, CollectionItemRemovedEventArgs e)
        {
            await CoreApplication.MainView.DispatcherQueue.EnqueueAsync(() =>
            {
                var itemToRemove = Items.FirstOrDefault((item) => item.CollectionItemViewModel.AssociatedItem.Path == e.itemChanged.AssociatedItem.Path);

                if (itemToRemove != null)
                {
                    Items.Remove(itemToRemove);
                }
            });
        }

        private async void AssociatedCollectionModel_OnCollectionItemRenamedEvent(object sender, CollectionItemRenamedEventArgs e)
        {
            await CoreApplication.MainView.DispatcherQueue.EnqueueAsync(async () =>
            { 
                var itemToChange = Items.FirstOrDefault((item) => item.CollectionItemViewModel.AssociatedItem.Path == e.oldPath);

                if (itemToChange != null)
                {
                    itemToChange.CollectionItemViewModel = e.itemChanged;

                    await itemToChange.UpdateDisplayName();
                }
            });
        }

        private void SearchControlModel_OnSearchCloseRequestedEvent(object sender, SearchCloseRequestedEventArgs e)
        {
            HideSearch();
        }

        private async void CollectionsControlViewModel_OnCollectionItemsInitializationFinishedEvent(object sender, CollectionItemsInitializationFinishedEventArgs e)
        {
            // Make sure the event was invoked for correct collection
            if (AssociatedCollectionModel == e.baseCollectionViewModel)
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
            _view.AssociatedCollectionModel.OnCollectionItemAddedEvent -= AssociatedCollectionModel_OnCollectionItemAddedEvent;
            _view.AssociatedCollectionModel.OnCollectionItemRemovedEvent -= AssociatedCollectionModel_OnCollectionItemRemovedEvent;
            _view.AssociatedCollectionModel.OnCollectionItemRenamedEvent -= AssociatedCollectionModel_OnCollectionItemRenamedEvent;

            if (_view.SearchControlModel != null)
            {
                _view.SearchControlModel.OnSearchCloseRequestedEvent -= SearchControlModel_OnSearchCloseRequestedEvent;
            }

            if (_collectionInitializationFinishedEventHooked)
            {
                CollectionsWidgetViewModel.OnCollectionItemsInitializationFinishedEvent -= CollectionsControlViewModel_OnCollectionItemsInitializationFinishedEvent;
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            UnhookEvents();

            if (SearchControlVisible)
            {
                AssociatedCollectionModel.SearchContext = SearchControlModel.ConstructSearchContext();
            }
            else
            {
                AssociatedCollectionModel.SearchContext = null;
            }
            
            Items.DisposeClear();
        }

        #endregion
    }
}
