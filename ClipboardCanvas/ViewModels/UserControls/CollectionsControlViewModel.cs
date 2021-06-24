using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Controls;
using System.Linq;
using Windows.Storage;

using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.EventArguments.Collections;
using ClipboardCanvas.EventArguments.Collections;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class CollectionsControlViewModel : ObservableObject, IDisposable
    {
        #region Private Members

        private static bool s_itemAddedInternally;

        private static int s_internalCollectionsCount;

        private DoubleClickWrapper _collectionDoubleClickWrapper;

        #endregion

        #region Public Properties

        // Collection of folders (canvas collection)
        public static ObservableCollection<CollectionViewModel> Items { get; private set; } = new ObservableCollection<CollectionViewModel>();

        private static CollectionViewModel _CurrentCollection;
        public static CollectionViewModel CurrentCollection
        {
            get => _CurrentCollection;
            set
            {
                if (value != _CurrentCollection)
                {
                    _CurrentCollection = value;

                    if (_CurrentCollection != null)
                    {
                        // Set selection property
                        foreach (var item in Items)
                        {
                            item.IsSelected = false;
                        }
                        _CurrentCollection.IsSelected = true;

                        CollectionsHelpers.UpdateLastSelectedCollectionSetting(_CurrentCollection);

                        OnCollectionSelectionChangedEvent?.Invoke(null, new CollectionSelectionChangedEventArgs(value));
                    }
                }
            }
        }

        public CollectionViewModel SelectedItem
        {
            get => CurrentCollection;
            set => CurrentCollection = value;
        }

        #endregion

        #region Events 

        /// <summary>
        /// Navigate to page with items in collection
        /// </summary>
        public static event EventHandler<CollectionOpenRequestedEventArgs> OnCollectionOpenRequestedEvent;

        public static event EventHandler<CollectionSelectionChangedEventArgs> OnCollectionSelectionChangedEvent;

        public static event EventHandler<CollectionRemovedEventArgs> OnCollectionRemovedEvent;

        public static event EventHandler<CollectionAddedEventArgs> OnCollectionAddedEvent;

        public static event EventHandler<CollectionItemsInitializationStartedEventArgs> OnCollectionItemsInitializationStartedEvent;

        public static event EventHandler<CollectionItemsInitializationFinishedEventArgs> OnCollectionItemsInitializationFinishedEvent;

        public static event EventHandler<OpenNewCanvasRequestedEventArgs> OnOpenNewCanvasRequestedEvent;

        public static event EventHandler<GoToHomePageRequestedEventArgs> OnGoToHomePageRequestedEvent;

        public static event EventHandler<CollectionErrorRaisedEventArgs> OnCollectionErrorRaisedEvent;

        public static event EventHandler<CheckCanvasPageNavigationRequestedEventArgs> OnCheckCanvasPageNavigationRequestedEvent;

        #endregion

        #region Commands

        public ICommand DragOverCommand { get; private set; }

        public ICommand DropCommand { get; private set; }

        public ICommand ItemClickCommand { get; private set; }

        #endregion

        #region Constructor

        public CollectionsControlViewModel()
        {
            HookEvents();

            _collectionDoubleClickWrapper = new DoubleClickWrapper(
                () => OnCollectionOpenRequestedEvent?.Invoke(this, new CollectionOpenRequestedEventArgs(SelectedItem)),
                TimeSpan.FromMilliseconds(Constants.Collections.DOUBLE_CLICK_DELAY_MILLISECONDS));

            // Create commands
            DragOverCommand = new AsyncRelayCommand<DragEventArgs>(DragOver);
            DropCommand = new AsyncRelayCommand<DragEventArgs>(Drop);
            ItemClickCommand = new RelayCommand<ItemClickEventArgs>(ItemClick);
        }

        #endregion

        #region Command Implementation

        private async Task DragOver(DragEventArgs e)
        {
            DragOperationDeferral deferral = null;

            try
            {
                deferral = e.GetDeferral();

                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    SafeWrapper<IReadOnlyList<IStorageItem>> items = await SafeWrapperRoutines.SafeWrapAsync(async () =>
                        await e.DataView.GetStorageItemsAsync());

                    if (items)
                    {
                        if (!items.Result.Any((item) => item is not IStorageFolder))
                        {
                            e.AcceptedOperation = DataPackageOperation.Move;
                            return;
                        }
                    }
                }

                e.AcceptedOperation = DataPackageOperation.None;
            }
            finally
            {
                e.Handled = true;
                deferral?.Complete();
            }
        }

        private async Task Drop(DragEventArgs e)
        {
            DragOperationDeferral deferral = null;

            try
            {
                deferral = e.GetDeferral();

                SafeWrapper<IReadOnlyList<IStorageItem>> items = await SafeWrapperRoutines.SafeWrapAsync(async () =>
                        await e.DataView.GetStorageItemsAsync());

                if (!items)
                {
                    return;
                }

                CollectionViewModel collection;
                foreach (var item in items.Result)
                {
                    // We retrieve the folder again this time using ToStorageItem<>() because items received cannot be modified - i.e. Renamed etc.
                    StorageFolder folder = await StorageHelpers.ToStorageItem<StorageFolder>(item.Path);

                    collection = new CollectionViewModel(folder);

                    await AddCollection(collection, true);
                }

                // We need to update saved collections because we suppressed that in AddCollection()
                CollectionsHelpers.UpdateSavedCollectionsSetting();
            }
            finally
            {
                e.Handled = true;
                deferral?.Complete();
            }
        }

        private void ItemClick(ItemClickEventArgs e)
        {
            _collectionDoubleClickWrapper.Click();
        }

        #endregion

        #region Event Handlers

        private static void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!s_itemAddedInternally && s_internalCollectionsCount < Items.Count)
            {
                CollectionsHelpers.UpdateSavedCollectionsSetting();
            }

            s_internalCollectionsCount = Items.Count;
        }

        private static void Container_OnCheckCanvasPageNavigationRequestedEvent(object sender, CheckCanvasPageNavigationRequestedEventArgs e)
        {
            OnCheckCanvasPageNavigationRequestedEvent?.Invoke(sender, e);
        }

        private static void Container_OnCollectionErrorRaisedEvent(object sender, CollectionErrorRaisedEventArgs e)
        {
            OnCollectionErrorRaisedEvent?.Invoke(sender, e);
        }

        private static void Container_OnGoToHomePageRequestedEvent(object sender, GoToHomePageRequestedEventArgs e)
        {
            OnGoToHomePageRequestedEvent?.Invoke(sender, e);
        }

        private static void Container_OnOpenNewCanvasRequestedEvent(object sender, OpenNewCanvasRequestedEventArgs e)
        {
            OnOpenNewCanvasRequestedEvent?.Invoke(sender, e);
        }

        private static void Container_OnRemoveCollectionRequestedEvent(object sender, RemoveCollectionRequestedEventArgs e)
        {
            RemoveCollection(e.collectionViewModel);
        }

        private static void Container_OnCheckRenameCollectionRequestedEvent(object sender, CheckRenameCollectionRequestedEventArgs e)
        {
            if (
                !Items.Any((item) => item.DisplayName == e.newName)
                && !e.collectionViewModel.isDefault
                && !string.IsNullOrWhiteSpace(e.newName)
                && !e.collectionViewModel.DisplayName.SequenceEqual(e.newName)
                )
            {
                e.canRename = true;
            }
            else
            {
                e.canRename = false;
            }
        }

        private static void Container_OnCollectionItemsInitializationFinishedEvent(object sender, CollectionItemsInitializationFinishedEventArgs e)
        {
            OnCollectionItemsInitializationFinishedEvent?.Invoke(sender, e);
        }

        private static void Container_OnCollectionItemsInitializationStartedEvent(object sender, CollectionItemsInitializationStartedEventArgs e)
        {
            OnCollectionItemsInitializationStartedEvent?.Invoke(sender, e);
        }

        #endregion

        #region Public Helpers

        public static async Task ReloadAllCollections()
        {
            List<string> savedCollectionPaths = App.AppSettings.CollectionLocationsSettings.SavedCollectionLocations;

            // Add default collection
            StorageFolder defaultCollectionFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Default Collection", CreationCollisionOption.OpenIfExists);

            if (defaultCollectionFolder == null)
            {
                Debugger.Break(); // That shouldn't even happen!
                throw new UnauthorizedAccessException("The default folder collection couldn't be retrieved!");
            }

            if (savedCollectionPaths == null)
            {
                savedCollectionPaths = new List<string>();
            }

            CollectionViewModel collection;

            collection = new CollectionViewModel(defaultCollectionFolder, true);
            await AddCollection(collection, true);

            foreach (var item in savedCollectionPaths)
            {
                collection = new CollectionViewModel(item);

                await AddCollection(collection, true);
            }

            // We need to update saved collections because we suppressed that in AddCollection()
            CollectionsHelpers.UpdateSavedCollectionsSetting();

            TrySetSelectedItem();

            OnCollectionItemsInitializationFinishedEvent?.Invoke(null, new CollectionItemsInitializationFinishedEventArgs(CurrentCollection));
        }

        public static void TrySetSelectedItem()
        {
            string lastSelectedCollection = App.AppSettings.CollectionLocationsSettings.LastSelectedCollection;

            if (string.IsNullOrWhiteSpace(lastSelectedCollection))
            {
                lastSelectedCollection = Constants.Collections.DEFAULT_COLLECTION_TOKEN;
                App.AppSettings.CollectionLocationsSettings.LastSelectedCollection = lastSelectedCollection;
            }

            var collection = Items.Where((item) => item.DangerousGetCollectionFolder()?.Path == lastSelectedCollection).FirstOrDefault();

            if (collection == null)
            {
                CurrentCollection = Items.Where((item) => item.isDefault).First();
                return;
            }
            else
            {
                CurrentCollection = collection;
            }
        }

        public static void RemoveCollection(CollectionViewModel container)
        {
            if (container.isDefault)
            {
                return;
            }

            container.OnCollectionItemsInitializationStartedEvent -= Container_OnCollectionItemsInitializationStartedEvent;
            container.OnCollectionItemsInitializationFinishedEvent -= Container_OnCollectionItemsInitializationFinishedEvent;
            container.OnCheckRenameCollectionRequestedEvent -= Container_OnCheckRenameCollectionRequestedEvent;
            container.OnRemoveCollectionRequestedEvent -= Container_OnRemoveCollectionRequestedEvent;
            container.OnOpenNewCanvasRequestedEvent -= Container_OnOpenNewCanvasRequestedEvent;
            container.OnGoToHomePageRequestedEvent -= Container_OnGoToHomePageRequestedEvent;
            container.OnCollectionErrorRaisedEvent -= Container_OnCollectionErrorRaisedEvent;
            container.OnCheckCanvasPageNavigationRequestedEvent -= Container_OnCheckCanvasPageNavigationRequestedEvent;

            int index = Items.IndexOf(container);
            container.Dispose();
            Items.RemoveAt(index);

            CollectionsHelpers.UpdateSavedCollectionsSetting();
            TrySetSelectedItem();
            CollectionsHelpers.UpdateLastSelectedCollectionSetting(CurrentCollection);
            OnCollectionRemovedEvent?.Invoke(null, new CollectionRemovedEventArgs(container));
        }

        public static async Task<bool> AddCollection(CollectionViewModel container, bool suppressSettingsUpdate = false)
        {
            if (Items.Any((item) => item.DangerousGetCollectionFolder()?.Path == container.CollectionFolderPath))
            {
                return false;
            } 

            container.OnCollectionItemsInitializationStartedEvent += Container_OnCollectionItemsInitializationStartedEvent;
            container.OnCollectionItemsInitializationFinishedEvent += Container_OnCollectionItemsInitializationFinishedEvent;
            container.OnCheckRenameCollectionRequestedEvent += Container_OnCheckRenameCollectionRequestedEvent;
            container.OnRemoveCollectionRequestedEvent += Container_OnRemoveCollectionRequestedEvent;
            container.OnOpenNewCanvasRequestedEvent += Container_OnOpenNewCanvasRequestedEvent;
            container.OnGoToHomePageRequestedEvent += Container_OnGoToHomePageRequestedEvent;
            container.OnCollectionErrorRaisedEvent += Container_OnCollectionErrorRaisedEvent;
            container.OnCheckCanvasPageNavigationRequestedEvent += Container_OnCheckCanvasPageNavigationRequestedEvent;

            await container.InitializeInnerStorageFolder();

            s_itemAddedInternally = true;
            Items.Add(container);
            s_itemAddedInternally = false;

            OnCollectionAddedEvent?.Invoke(null, new CollectionAddedEventArgs(container));

            if (!suppressSettingsUpdate)
            {
                CollectionsHelpers.UpdateSavedCollectionsSetting();
            }
            
            return true;
        }

        public static async Task<bool> AddCollectionViaUi()
        {
            StorageFolder folder = await App.DialogService.PickSingleFolder();

            // We retrieve the folder again this time using ToStorageItem<>() because items picked by the FilePicker
            // or FolderPicker cannot be modified - i.e. Renamed etc.
            folder = await StorageHelpers.ToStorageItem<StorageFolder>(folder?.Path);

            if (folder == null)
            {
                return false; // User didn't pick any folder or couldn't retrieve the folder after it's been picked
            }

            var collection = new CollectionViewModel(folder);

            return await AddCollection(collection);
        }

        #endregion

        #region Event Hooks

        private void HookEvents()
        {
            UnhookEvents();
            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void UnhookEvents()
        {
            Items.CollectionChanged -= Items_CollectionChanged;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _collectionDoubleClickWrapper?.Dispose();
            UnhookEvents();
        }

        #endregion
    }
}
