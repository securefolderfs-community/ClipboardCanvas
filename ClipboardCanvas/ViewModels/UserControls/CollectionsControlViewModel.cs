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

                        OnCollectionSelectionChanged?.Invoke(null, new CollectionSelectionChangedEventArgs(value));
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
        public static event EventHandler<CollectionOpenRequestedEventArgs> OnCollectionOpenRequested;

        public static event EventHandler<CollectionSelectionChangedEventArgs> OnCollectionSelectionChanged;

        public static event EventHandler<CollectionRemovedEventArgs> OnCollectionRemoved;

        public static event EventHandler<CollectionAddedEventArgs> OnCollectionAdded;

        public static event EventHandler<CollectionItemsRefreshRequestedEventArgs> OnCollectionItemsRefreshRequested;

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
            OnCollectionOpenRequested?.Invoke(this, new CollectionOpenRequestedEventArgs(SelectedItem));
        }

        #endregion

        #region Event Handlers

        private static void Container_OnRemoveCollectionRequestedEvent(object sender, RemoveCollectionRequestedEventArgs e)
        {
            RemoveCollection(e.containerViewModel);
        }

        private static async void Container_OnRenameCollectionRequestedEvent(object sender, RenameCollectionRequestedEventArgs e)
        {
            if (e.containerViewModel.isDefault)
            {
                e.renamed = false;
                return;
            }
            if (string.IsNullOrWhiteSpace(e.newName))
            {
                e.renamed = false;
                return;
            }
            else if (e.containerViewModel.DisplayName.SequenceEqual(e.newName))
            {
                e.renamed = false;
                return;
            }

            bool canRename = !Items.Any((item) => item.DisplayName == e.newName);

            if (canRename)
            {
                e.renamed = true;
                await e.containerViewModel.DangerousGetCollectionFolder().RenameAsync(e.newName, NameCollisionOption.FailIfExists);
            }
        }

        private static void Container_OnItemsRefreshRequestedEvent(object sender, ItemsRefreshRequestedEventArgs e)
        {
            OnCollectionItemsRefreshRequested?.Invoke(null, new CollectionItemsRefreshRequestedEventArgs(e.containerViewModel));
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

            collection = new CollectionsContainerViewModel(defaultCollectionFolder.Path, true);
            collection.InitInnerStorageFolder(defaultCollectionFolder);
            AddCollection(collection);

            foreach (var item in savedCollectionPaths)
            {
                collection = new CollectionsContainerViewModel(item);
                collection.InitInnerStorageFolder(defaultCollectionFolder);

                AddCollection(collection);
            }

            TrySetSelectedItem();

            OnCollectionItemsRefreshRequested?.Invoke(null, new CollectionItemsRefreshRequestedEventArgs(CurrentCollection));
        }

        public static void TrySetSelectedItem()
        {
            string lastSelectedCollection = App.AppSettings.CollectionLocationsSettings.LastSelectedCollection;

            if (string.IsNullOrWhiteSpace(lastSelectedCollection))
            {
                lastSelectedCollection = Constants.Collections.DEFAULT_COLLECTION_TOKEN;
                App.AppSettings.CollectionLocationsSettings.LastSelectedCollection = lastSelectedCollection;
            }

            var collection = Items.Where((item) => item.collectionFolderPath == lastSelectedCollection).FirstOrDefault();

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

            container.OnItemsRefreshRequestedEvent -= Container_OnItemsRefreshRequestedEvent;
            container.OnRenameCollectionRequestedEvent -= Container_OnRenameCollectionRequestedEvent;
            container.OnRemoveCollectionRequestedEvent -= Container_OnRemoveCollectionRequestedEvent;

            int index = Items.IndexOf(container);
            container.Dispose();
            Items.RemoveAt(index);

            CollectionsHelpers.UpdateSavedCollectionLocationsSetting();
            TrySetSelectedItem();
            CollectionsHelpers.UpdateLastSelectedCollectionSetting(CurrentCollection);
            OnCollectionRemoved?.Invoke(null, new CollectionRemovedEventArgs(container));
        }

        public static bool AddCollection(CollectionsContainerViewModel container)
        {
            if (Items.Any((item) => item.collectionFolderPath == container.collectionFolderPath))
            {
                return false;
            }

            container.OnItemsRefreshRequestedEvent += Container_OnItemsRefreshRequestedEvent;
            container.OnRenameCollectionRequestedEvent += Container_OnRenameCollectionRequestedEvent;
            container.OnRemoveCollectionRequestedEvent += Container_OnRemoveCollectionRequestedEvent;

            Items.Add(container);

            OnCollectionAdded?.Invoke(null, new CollectionAddedEventArgs(container));

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

            var collection = new CollectionsContainerViewModel(folder.Path);
            if (AddCollection(collection))
            {
                //collection.StartRename();
                return true;
            }

            return false;
        }

        #endregion
    }
}
