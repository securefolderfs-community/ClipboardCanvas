using System;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Xaml.Input;

using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Models;
using System.Linq;
using Windows.Storage.Pickers;
using System.IO;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.EventArguments.CollectionControl;
using ClipboardCanvas.EventArguments.CollectionsContainer;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class CollectionsControlViewModel : ObservableObject
    {
        #region Public Properties

        // Collection of folders (canvas collection)
        public static ObservableCollection<CollectionsContainerViewModel> Items { get; private set; } = new ObservableCollection<CollectionsContainerViewModel>();

        private static CollectionsContainerViewModel _CurrentCollection;
        public static CollectionsContainerViewModel CurrentCollection
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

        public CollectionsContainerViewModel SelectedItem
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

        #endregion

        #region Commands

        public ICommand DoubleTappedCommand { get; private set; }

        #endregion

        #region Constructor

        public CollectionsControlViewModel()
        {
            // Create commands
            DoubleTappedCommand = new RelayCommand<DoubleTappedRoutedEventArgs>(DoubleTapped);
        }

        #endregion

        #region Command Implementation

        private void DoubleTapped(DoubleTappedRoutedEventArgs e)
        {
            OnCollectionOpenRequestedEvent?.Invoke(this, new CollectionOpenRequestedEventArgs(SelectedItem));
        }

        #endregion

        #region Event Handlers

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
            RemoveCollection(e.containerViewModel);
        }

        private static void Container_OnCheckRenameCollectionRequestedEvent(object sender, CheckRenameCollectionRequestedEventArgs e)
        {
            if (
                !Items.Any((item) => item.DisplayName == e.newName)
                && !e.containerViewModel.isDefault
                && !string.IsNullOrWhiteSpace(e.newName)
                && !e.containerViewModel.DisplayName.SequenceEqual(e.newName)
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

            SafetyExtensions.InitNull(ref savedCollectionPaths); // Check for any collections

            CollectionsContainerViewModel collection;

            collection = new CollectionsContainerViewModel(defaultCollectionFolder, true);
            await AddCollection(collection);

            foreach (var item in savedCollectionPaths)
            {
                collection = new CollectionsContainerViewModel(item);

                await AddCollection(collection);
            }

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

        public static void RemoveCollection(CollectionsContainerViewModel container)
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

            int index = Items.IndexOf(container);
            container.Dispose();
            Items.RemoveAt(index);

            CollectionsHelpers.UpdateSavedCollectionLocationsSetting();
            TrySetSelectedItem();
            CollectionsHelpers.UpdateLastSelectedCollectionSetting(CurrentCollection);
            OnCollectionRemovedEvent?.Invoke(null, new CollectionRemovedEventArgs(container));
        }

        public static async Task<bool> AddCollection(CollectionsContainerViewModel container)
        {
            if (Items.Any((item) => item.DangerousGetCollectionFolder()?.Path == container.DangerousGetCollectionFolder()?.Path))
            {
                return false;
            }

            container.OnCollectionItemsInitializationStartedEvent += Container_OnCollectionItemsInitializationStartedEvent;
            container.OnCollectionItemsInitializationFinishedEvent += Container_OnCollectionItemsInitializationFinishedEvent;
            container.OnCheckRenameCollectionRequestedEvent += Container_OnCheckRenameCollectionRequestedEvent;
            container.OnRemoveCollectionRequestedEvent += Container_OnRemoveCollectionRequestedEvent;
            container.OnOpenNewCanvasRequestedEvent += Container_OnOpenNewCanvasRequestedEvent;
            container.OnGoToHomePageRequestedEvent += Container_OnGoToHomePageRequestedEvent;
            await container.InitializeInnerStorageFolder();

            Items.Add(container);

            OnCollectionAddedEvent?.Invoke(null, new CollectionAddedEventArgs(container));

            CollectionsHelpers.UpdateSavedCollectionLocationsSetting();

            return true;
        }

        public static async Task<bool> AddCollectionViaUi()
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            if (folder == null)
            {
                return false; // User didn't pick any folder
            }

            var collection = new CollectionsContainerViewModel(folder);
            if (await AddCollection(collection))
            {
                //collection.StartRename();
                return true;
            }

            return false;
        }

        #endregion
    }
}
