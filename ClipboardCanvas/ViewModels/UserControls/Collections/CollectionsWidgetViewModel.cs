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
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.EventArguments.Collections;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Interfaces.Collections;
using ClipboardCanvas.Services;
using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.UserControls.InAppNotifications;

namespace ClipboardCanvas.ViewModels.UserControls.Collections
{
    public class CollectionsWidgetViewModel : ObservableObject, IDisposable
    {
        #region Private Members

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

                        SettingsSerializationHelpers.UpdateLastSelectedCollectionSetting(_CurrentCollection);

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

        #endregion

        #region Constructor

        public CollectionsWidgetViewModel()
        {
            HookEvents();

            // Create commands
            DragOverCommand = new AsyncRelayCommand<DragEventArgs>(DragOver);
            DropCommand = new AsyncRelayCommand<DragEventArgs>(Drop);
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

                if (App.IsInRestrictedAccessMode)
                {
                    IDialogService dialogService = Ioc.Default.GetService<IDialogService>();

                    IInAppNotification notification = dialogService.GetNotification();
                    notification.ViewModel.NotificationText = "Cannot add collections in Restricted Access mode.";
                    notification.ViewModel.ShownButtons = Enums.InAppNotificationButtonType.OkButton;
                    notification.Show(Constants.UI.Notifications.NOTIFICATION_DEFAULT_SHOW_TIME);

                    return;
                }

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
                        await AddCollection(standardCollectionViewModel, null, true);
                    }
                }

                // We need to update saved collections because we suppressed that in AddCollection()
                SettingsSerializationHelpers.UpdateSavedCollectionsSetting();
            }
            finally
            {
                e.Handled = true;
                deferral?.Complete();
            }
        }

        #endregion

        #region Event Handlers

        private static void Collections_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!s_itemAddedInternally && s_internalCollectionsCount < Collections.Count)
            {
                SettingsSerializationHelpers.UpdateSavedCollectionsSetting();
            }

            s_internalCollectionsCount = Collections.Count;
        }

        private static async void CollectionRemovable_OnRemoveCollectionRequestedEvent(object sender, RemoveCollectionRequestedEventArgs e)
        {
            await RemoveCollection(e.baseCollectionViewModel);
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

        private static void BaseCollectionViewModel_OnCollectionOpenRequestedEvent(object sender, CollectionOpenRequestedEventArgs e)
        {
            OnCollectionOpenRequestedEvent?.Invoke(sender, e);
        }

        #endregion

        #region Public Helpers

        public void OpenItem(BaseCollectionViewModel collectionViewModel)
        {
            OnCollectionOpenRequestedEvent?.Invoke(this, new CollectionOpenRequestedEventArgs(collectionViewModel));
        }

        public static async Task ReloadAllCollections()
        {
            ICollectionsSettingsService collectionsSettings = Ioc.Default.GetService<ICollectionsSettingsService>();

            List<CollectionConfigurationModel> collectionConfigurations = collectionsSettings.SavedCollections;

            // Add default collection
            StorageFolder defaultCollectionFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Default Collection", CreationCollisionOption.OpenIfExists);

            if (defaultCollectionFolder == null)
            {
                Debugger.Break(); // That shouldn't even happen!
                throw new UnauthorizedAccessException("The default folder collection couldn't be retrieved!");
            }

            if (collectionConfigurations == null)
            {
                collectionConfigurations = new List<CollectionConfigurationModel>();
                collectionsSettings.SavedCollections = collectionConfigurations;
            }

            bool defaultCollectionAdded = false;

            foreach (var item in collectionConfigurations)
            {
                BaseCollectionViewModel baseCollection;

                if (!defaultCollectionAdded && item.collectionPath == Constants.Collections.DEFAULT_COLLECTION_TOKEN)
                {
                    defaultCollectionAdded = true;
                    baseCollection = new DefaultCollectionViewModel(defaultCollectionFolder);
                }
                else
                {
                    baseCollection = new StandardCollectionViewModel(item.collectionPath);
                }

                await AddCollection(baseCollection, item, true);
            }

            if (!defaultCollectionAdded)
            {
                // Add default collection if it hasn't been added (it wasn't found in collections_settings)
                await AddCollection(new DefaultCollectionViewModel(defaultCollectionFolder), null, true);
            }

            // We need to update saved collections because we suppressed that in AddCollection()
            SettingsSerializationHelpers.UpdateSavedCollectionsSetting();

            FallbackSetSelectedCollection();

            OnCollectionItemsInitializationFinishedEvent?.Invoke(null, new CollectionItemsInitializationFinishedEventArgs(CurrentCollection));
        }

        public static void FallbackSetSelectedCollection()
        {
            ICollectionsSettingsService collectionsSettings = Ioc.Default.GetService<ICollectionsSettingsService>();

            string lastSelectedCollection = collectionsSettings.LastSelectedCollection;

            if (string.IsNullOrWhiteSpace(lastSelectedCollection))
            {
                // The last selected collection setting is not set
                lastSelectedCollection = Constants.Collections.DEFAULT_COLLECTION_TOKEN;
                collectionsSettings.LastSelectedCollection = lastSelectedCollection;
            }

            if (lastSelectedCollection == Constants.Collections.DEFAULT_COLLECTION_TOKEN)
            {
                // Set current collection to default collection
                CurrentCollection = Collections.First((item) => item is DefaultCollectionViewModel);
            }
            else
            {
                // Find the collection with the matching lastSelectedCollection setting
                BaseCollectionViewModel baseCollectionViewModel = Collections.SingleOrDefault((item) => item.CollectionPath == lastSelectedCollection);

                if (baseCollectionViewModel != null)
                {
                    CurrentCollection = baseCollectionViewModel;
                }
                else
                {
                    // Fallback to default collection
                    CurrentCollection = Collections.First((item) => item is DefaultCollectionViewModel);
                }
            }
        }

        public static BaseCollectionViewModel FindCollection(CollectionConfigurationModel collectionConfigurationModel)
        {
            return Collections.FirstOrDefault((item) => item.ConstructConfigurationModel().collectionPath == collectionConfigurationModel.collectionPath);
        }

        public static BaseCollectionViewModel FindCollection(string collectionPath)
        {
            return Collections.FirstOrDefault((item) => item.CollectionPath == collectionPath);
        }

        public static async Task<bool> AddCollection(BaseCollectionViewModel baseCollectionViewModel, CollectionConfigurationModel configurationModel, bool suppressSettingsUpdate = false)
        {
            // If collections already contain a collection with the same path
            if (Collections.Any((item) => item.CollectionPath == baseCollectionViewModel.CollectionPath))
            {
                return false;
            }

            // Hook events
            baseCollectionViewModel.OnCollectionOpenRequestedEvent += BaseCollectionViewModel_OnCollectionOpenRequestedEvent;
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
            await baseCollectionViewModel.InitializeIconIfSet(configurationModel);


            s_itemAddedInternally = true;
            Collections.Add(baseCollectionViewModel);
            s_itemAddedInternally = false;

            OnCollectionAddedEvent?.Invoke(null, new CollectionAddedEventArgs(baseCollectionViewModel));

            if (!suppressSettingsUpdate)
            {
                SettingsSerializationHelpers.UpdateSavedCollectionsSetting();
            }

            return true;
        }

        public static async Task RemoveCollection(BaseCollectionViewModel baseCollectionViewModel)
        {
            if (baseCollectionViewModel is DefaultCollectionViewModel)
            {
                return;
            }

            // Unhook events
            baseCollectionViewModel.OnCollectionOpenRequestedEvent -= BaseCollectionViewModel_OnCollectionOpenRequestedEvent;
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

            Collections.Remove(baseCollectionViewModel);

            // Also remove the icon if there was any
            await baseCollectionViewModel.RemoveCollectionIcon();

            baseCollectionViewModel.Dispose();

            SettingsSerializationHelpers.UpdateSavedCollectionsSetting();
            FallbackSetSelectedCollection();
            SettingsSerializationHelpers.UpdateLastSelectedCollectionSetting(CurrentCollection);

            OnCollectionRemovedEvent?.Invoke(null, new CollectionRemovedEventArgs(baseCollectionViewModel));
        }

        public static async Task<bool> AddCollectionViaUi()
        {
            IDialogService dialogService = Ioc.Default.GetService<IDialogService>();

            if (App.IsInRestrictedAccessMode)
            {
                IInAppNotification notification = dialogService.GetNotification();
                notification.ViewModel.NotificationText = "Cannot add collections in Restricted Access mode.";
                notification.ViewModel.ShownButtons = Enums.InAppNotificationButtonType.OkButton;
                notification.Show(Constants.UI.Notifications.NOTIFICATION_DEFAULT_SHOW_TIME);

                return false;
            }

            StorageFolder folder = await dialogService.PickSingleFolder();

            // We retrieve the folder again this time using ToStorageItem<>() because items picked by the FilePicker
            // or FolderPicker cannot be modified - i.e. Renamed etc.
            folder = await StorageHelpers.ToStorageItem<StorageFolder>(folder?.Path);

            if (folder == null)
            {
                return false; // User didn't pick any folder or couldn't retrieve the folder after it's been picked
            }

            StandardCollectionViewModel standardCollectionViewModel = new StandardCollectionViewModel(folder);

            return await AddCollection(standardCollectionViewModel, null);
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
            UnhookEvents();
        }

        #endregion
    }
}
