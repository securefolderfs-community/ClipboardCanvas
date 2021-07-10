using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.EventArguments.Collections;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Interfaces.Collections;

namespace ClipboardCanvas.ViewModels.UserControls.Collections
{
    public class CollectionsControlViewModel : ObservableObject, IDisposable
    {
        #region Private Members

        private DoubleClickWrapper _collectionDoubleClickWrapper;

        private static bool s_itemAddedInternally;

        private static int s_internalCollectionsCount;

        #endregion

        #region Public Properties

        public static ObservableCollection<BaseCollectionViewModel> Collections { get; private set; } = new ObservableCollection<BaseCollectionViewModel>();

        private static BaseCollectionViewModel _CurrentCollection;
        public static BaseCollectionViewModel CurrentCollection
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
                        foreach (var item in Collections)
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

        public BaseCollectionViewModel SelectedItem
        {
            get => CurrentCollection;
            set => CurrentCollection = value;
        }

        #endregion

        #region Events

        public static event EventHandler<CollectionOpenRequestedEventArgs> OnCollectionOpenRequestedEvent;

        public static event EventHandler<CollectionSelectionChangedEventArgs> OnCollectionSelectionChangedEvent;

        public static event EventHandler<CollectionRemovedEventArgs> OnCollectionRemovedEvent;

        public static event EventHandler<CollectionAddedEventArgs> OnCollectionAddedEvent;

        // Collection events

        public static event EventHandler<CollectionItemsInitializationStartedEventArgs> OnCollectionItemsInitializationStartedEvent;

        public static event EventHandler<CollectionItemsInitializationFinishedEventArgs> OnCollectionItemsInitializationFinishedEvent;

        public static event EventHandler<OpenNewCanvasRequestedEventArgs> OnOpenNewCanvasRequestedEvent;

        public static event EventHandler<GoToHomepageRequestedEventArgs> OnGoToHomepageRequestedEvent;

        public static event EventHandler<CollectionErrorRaisedEventArgs> OnCollectionErrorRaisedEvent;

        public static event EventHandler<CanvasLoadFailedEventArgs> OnCanvasLoadFailedEvent;

        public static event EventHandler<TipTextUpdateRequestedEventArgs> OnTipTextUpdateRequestedEvent;

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
                () => OnCollectionOpenRequestedEvent?.Invoke(this, new CollectionOpenRequestedEventArgs(CurrentCollection)),
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

                foreach (var item in items.Result)
                {
                    // We retrieve the folder again this time using ToStorageItem<>() because items received cannot be modified - i.e. Renamed etc.
                    StorageFolder folder = await StorageHelpers.ToStorageItem<StorageFolder>(item.Path);

                    if (folder != null)
                    {
                        StandardCollectionViewModel standardCollectionViewModel = new StandardCollectionViewModel(folder);
                        await AddCollection(standardCollectionViewModel, true);
                    }
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
        
        private static void Collections_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!s_itemAddedInternally && s_internalCollectionsCount < Collections.Count)
            {
                CollectionsHelpers.UpdateSavedCollectionsSetting();
            }

            s_internalCollectionsCount = Collections.Count;
        }

        private static void CollectionRemovable_OnRemoveCollectionRequestedEvent(object sender, RemoveCollectionRequestedEventArgs e)
        {
            RemoveCollection(e.baseCollectionViewModel);
        }

        private static void CollectionNameEditable_OnCheckRenameCollectionRequestedEvent(object sender, CheckRenameCollectionRequestedEventArgs e)
        {
            if (
                e.baseCollectionViewModel is ICollectionNameEditable
               && !string.IsNullOrWhiteSpace(e.newName)
               && !Collections.Any((item) => item.DisplayName == e.newName)
               && !e.baseCollectionViewModel.DisplayName.SequenceEqual(e.newName)
               )
            {
                e.canRename = true;
            }
            else
            {
                e.canRename = false;
            }
        }

        private static void BaseCollectionViewModel_OnTipTextUpdateRequestedEvent(object sender, TipTextUpdateRequestedEventArgs e)
        {
            OnTipTextUpdateRequestedEvent?.Invoke(sender, e);
        }

        private static void BaseCollectionViewModel_OnCollectionItemsInitializationFinishedEvent(object sender, CollectionItemsInitializationFinishedEventArgs e)
        {
            OnCollectionItemsInitializationFinishedEvent?.Invoke(sender, e);
        }

        private static void BaseCollectionViewModel_OnCollectionItemsInitializationStartedEvent(object sender, CollectionItemsInitializationStartedEventArgs e)
        {
            OnCollectionItemsInitializationStartedEvent?.Invoke(sender, e);
        }

        private static void BaseCollectionViewModel_OnCollectionErrorRaisedEvent(object sender, CollectionErrorRaisedEventArgs e)
        {
            OnCollectionErrorRaisedEvent?.Invoke(sender, e);
        }

        private static void BaseCollectionViewModel_OnGoToHomepageRequestedEvent(object sender, GoToHomepageRequestedEventArgs e)
        {
            OnGoToHomepageRequestedEvent?.Invoke(sender, e);
        }

        private static void BaseCollectionViewModel_OnCanvasLoadFailedEvent(object sender, CanvasLoadFailedEventArgs e)
        {
            OnCanvasLoadFailedEvent?.Invoke(sender, e);
        }

        private static void BaseCollectionViewModel_OnOpenNewCanvasRequestedEvent(object sender, OpenNewCanvasRequestedEventArgs e)
        {
            OnOpenNewCanvasRequestedEvent?.Invoke(sender, e);
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

            DefaultCollectionViewModel defaultCollectionViewModel = new DefaultCollectionViewModel(defaultCollectionFolder);
            bool defaultCollectionAdded = false;

            foreach (var item in savedCollectionPaths)
            {
                if (!defaultCollectionAdded && item == Constants.Collections.DEFAULT_COLLECTION_TOKEN)
                {
                    defaultCollectionAdded = true;
                    await AddCollection(defaultCollectionViewModel, true);
                }
                else
                {
                    StandardCollectionViewModel standardCollectionViewModel = new StandardCollectionViewModel(item);
                    await AddCollection(standardCollectionViewModel, true);
                }
            }

            if (!defaultCollectionAdded)
            {
                // Add default collection if it hasn't been added
                await AddCollection(defaultCollectionViewModel, true);
            }

            // We need to update saved collections because we suppressed that in AddCollection()
            CollectionsHelpers.UpdateSavedCollectionsSetting();

            FallbackSetSelectedCollection();

            OnCollectionItemsInitializationFinishedEvent?.Invoke(null, new CollectionItemsInitializationFinishedEventArgs(CurrentCollection));
        }

        public static void FallbackSetSelectedCollection()
        {
            string lastSelectedCollection = App.AppSettings.CollectionLocationsSettings.LastSelectedCollection;

            if (string.IsNullOrWhiteSpace(lastSelectedCollection))
            {
                // The last selected collection setting is not set
                lastSelectedCollection = Constants.Collections.DEFAULT_COLLECTION_TOKEN;
                App.AppSettings.CollectionLocationsSettings.LastSelectedCollection = lastSelectedCollection;
            }

            if (lastSelectedCollection == Constants.Collections.DEFAULT_COLLECTION_TOKEN)
            {
                // Set current collection to default collection
                CurrentCollection = Collections.Where((item) => item is DefaultCollectionViewModel).First();
            }
            else
            {
                // Find the collection with the matching lastSelectedCollection setting
                BaseCollectionViewModel baseCollectionViewModel = Collections.Where((item) => item.CollectionPath == lastSelectedCollection).FirstOrDefault();

                if (baseCollectionViewModel != null)
                {
                    CurrentCollection = baseCollectionViewModel;
                }
                else
                {
                    // Fallback to default collection
                    CurrentCollection = Collections.Where((item) => item is DefaultCollectionViewModel).First();
                }
            }
        }

        public static async Task<bool> AddCollection(BaseCollectionViewModel baseCollectionViewModel, bool suppressSettingsUpdate = false)
        {
            // If collections already contain a collection with the same path
            if (Collections.Any((item) => item.CollectionPath == baseCollectionViewModel.CollectionPath))
            {
                return false;
            }

            // Hook events
            baseCollectionViewModel.OnOpenNewCanvasRequestedEvent += BaseCollectionViewModel_OnOpenNewCanvasRequestedEvent;
            baseCollectionViewModel.OnCanvasLoadFailedEvent += BaseCollectionViewModel_OnCanvasLoadFailedEvent;
            baseCollectionViewModel.OnGoToHomepageRequestedEvent += BaseCollectionViewModel_OnGoToHomepageRequestedEvent;
            baseCollectionViewModel.OnCollectionErrorRaisedEvent += BaseCollectionViewModel_OnCollectionErrorRaisedEvent;
            baseCollectionViewModel.OnCollectionItemsInitializationStartedEvent += BaseCollectionViewModel_OnCollectionItemsInitializationStartedEvent;
            baseCollectionViewModel.OnCollectionItemsInitializationFinishedEvent += BaseCollectionViewModel_OnCollectionItemsInitializationFinishedEvent;
            baseCollectionViewModel.OnTipTextUpdateRequestedEvent += BaseCollectionViewModel_OnTipTextUpdateRequestedEvent;

            // Hook extension interfaces events
            if (baseCollectionViewModel is ICollectionNameEditable collectionNameEditable)
            {
                collectionNameEditable.OnCheckRenameCollectionRequestedEvent += CollectionNameEditable_OnCheckRenameCollectionRequestedEvent;
            }
            if (baseCollectionViewModel is ICollectionRemovable collectionRemovable)
            {
                collectionRemovable.OnRemoveCollectionRequestedEvent += CollectionRemovable_OnRemoveCollectionRequestedEvent;
            }

            await baseCollectionViewModel.InitializeCollectionFolder();

            s_itemAddedInternally = true;
            Collections.Add(baseCollectionViewModel);
            s_itemAddedInternally = false;

            OnCollectionAddedEvent?.Invoke(null, new CollectionAddedEventArgs(baseCollectionViewModel));

            if (!suppressSettingsUpdate)
            {
                CollectionsHelpers.UpdateSavedCollectionsSetting();
            }

            return true;
        }

        public static void RemoveCollection(BaseCollectionViewModel baseCollectionViewModel)
        {
            if (baseCollectionViewModel is DefaultCollectionViewModel)
            {
                return;
            }

            // Hook events
            baseCollectionViewModel.OnOpenNewCanvasRequestedEvent -= BaseCollectionViewModel_OnOpenNewCanvasRequestedEvent;
            baseCollectionViewModel.OnCanvasLoadFailedEvent -= BaseCollectionViewModel_OnCanvasLoadFailedEvent;
            baseCollectionViewModel.OnGoToHomepageRequestedEvent -= BaseCollectionViewModel_OnGoToHomepageRequestedEvent;
            baseCollectionViewModel.OnCollectionErrorRaisedEvent -= BaseCollectionViewModel_OnCollectionErrorRaisedEvent;
            baseCollectionViewModel.OnCollectionItemsInitializationStartedEvent -= BaseCollectionViewModel_OnCollectionItemsInitializationStartedEvent;
            baseCollectionViewModel.OnCollectionItemsInitializationFinishedEvent -= BaseCollectionViewModel_OnCollectionItemsInitializationFinishedEvent;
            baseCollectionViewModel.OnTipTextUpdateRequestedEvent -= BaseCollectionViewModel_OnTipTextUpdateRequestedEvent;

            // Unhook extension interfaces events
            if (baseCollectionViewModel is ICollectionNameEditable collectionNameEditable)
            {
                collectionNameEditable.OnCheckRenameCollectionRequestedEvent -= CollectionNameEditable_OnCheckRenameCollectionRequestedEvent;
            }
            if (baseCollectionViewModel is ICollectionRemovable collectionRemovable)
            {
                collectionRemovable.OnRemoveCollectionRequestedEvent -= CollectionRemovable_OnRemoveCollectionRequestedEvent;
            }

            baseCollectionViewModel.Dispose();
            Collections.Remove(baseCollectionViewModel);

            CollectionsHelpers.UpdateSavedCollectionsSetting();
            FallbackSetSelectedCollection();
            CollectionsHelpers.UpdateLastSelectedCollectionSetting(CurrentCollection);

            OnCollectionRemovedEvent?.Invoke(null, new CollectionRemovedEventArgs(baseCollectionViewModel));
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

            StandardCollectionViewModel standardCollectionViewModel = new StandardCollectionViewModel(folder);

            return await AddCollection(standardCollectionViewModel);
        }

        #endregion

        #region Event Hooks

        private void HookEvents()
        {
            UnhookEvents();
            Collections.CollectionChanged += Collections_CollectionChanged;
        }

        private void UnhookEvents()
        {
            Collections.CollectionChanged -= Collections_CollectionChanged;
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
