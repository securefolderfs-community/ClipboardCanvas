using System;
using Windows.Storage;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.UI.Xaml.Input;
using Windows.System;
using Windows.UI.Xaml;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using ClipboardCanvas.Models;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.ReferenceItems;
using ClipboardCanvas.EventArguments.CollectionsContainer;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class CollectionsContainerViewModel : ObservableObject, ICollectionsContainerModel, IDisposable
    {
        #region Private Members

        private StorageFolder _innerStorageFolder;

        private int _currentIndex;

        private string _collectionFolderPath;

        #endregion

        #region Public Members

        public readonly bool isDefault;

        #endregion

        #region Public Properties

        public List<CollectionsContainerItemViewModel> Items { get; private set; }

        public bool CanOpenCollection { get; private set; } = true;

        public bool IsOnNewCanvas => this._currentIndex == Items.Count;

        public bool CanvasInitialized { get; private set; }

        public bool CanvasInitializing { get; private set; }

        public string Name
        {
            get => Path.GetFileName(_collectionFolderPath);
        }

        public string DisplayName
        {
            get => Name;
        }

        private bool _IsLoadingItems;
        public bool IsLoadingItems
        {
            get => _IsLoadingItems;
            set => SetProperty(ref _IsLoadingItems, value);
        }

        private string _EditBoxText;
        public string EditBoxText
        {
            get => _EditBoxText;
            set => SetProperty(ref _EditBoxText, value);
        }

        private bool _IsEditingName;
        public bool IsEditingName
        {
            get => _IsEditingName;
            private set => SetProperty(ref _IsEditingName, value);
        }

        private bool _IsSelected = false;
        public bool IsSelected
        {
            get => _IsSelected;
            set => SetProperty(ref _IsSelected, value);
        }

        private Visibility _ErrorIconVisibility = Visibility.Collapsed;
        public Visibility ErrorIconVisibility
        {
            get => _ErrorIconVisibility;
            set => SetProperty(ref _ErrorIconVisibility, value);
        }

        public bool IsFilled => _currentIndex < Items.Count;

        public ICollectionsContainerItemModel CurrentCanvas => Items.Count == _currentIndex ? null : this.Items[_currentIndex];

        #endregion

        #region Events

        public event EventHandler<CollectionItemsInitializationStartedEventArgs> OnCollectionItemsInitializationStartedEvent;

        public event EventHandler<CollectionItemsInitializationFinishedEventArgs> OnCollectionItemsInitializationFinishedEvent;

        public event EventHandler<CheckRenameCollectionRequestedEventArgs> OnCheckRenameCollectionRequestedEvent;

        public event EventHandler<RemoveCollectionRequestedEventArgs> OnRemoveCollectionRequestedEvent;

        public event EventHandler<OpenNewCanvasRequestedEventArgs> OnOpenNewCanvasRequestedEvent;

        public event EventHandler<GoToHomePageRequestedEventArgs> OnGoToHomePageRequestedEvent;

        #endregion

        #region Commands

        public ICommand OpenCollectionLocationCommand { get; private set; }

        public ICommand RefreshCollectionCommand { get; private set; }

        public ICommand RenameCollectionCommand { get; private set; }

        public ICommand RemoveCollectionCommand { get; private set; }

        public ICommand EditBoxKeyDownCommand { get; private set; }

        public ICommand EditBoxLostFocusCommand { get; private set; }

        #endregion

        #region Constructor

        public CollectionsContainerViewModel(string collectionFolderPath, bool isDefault = false)
            : this (collectionFolderPath, null, isDefault)
        {
        }

        public CollectionsContainerViewModel(StorageFolder collectionFolder, bool isDefault = false)
            : this (null, collectionFolder, isDefault)
        {
        }

        private CollectionsContainerViewModel(string collectionFolderPath, StorageFolder collectionFolder, bool isDefault = false)
        {
            if (!string.IsNullOrEmpty(collectionFolderPath))
            {
                this._collectionFolderPath = collectionFolderPath;
            }
            else
            {
                this._collectionFolderPath = collectionFolder?.Path;
            }
            this._innerStorageFolder = collectionFolder;
            this.isDefault = isDefault;

            this.Items = new List<CollectionsContainerItemViewModel>();

            OnPropertyChanged(nameof(DisplayName));

            // Create commands
            OpenCollectionLocationCommand = new RelayCommand(OpenCollectionLocation);
            RefreshCollectionCommand = new RelayCommand(RefreshCollection);
            RenameCollectionCommand = new RelayCommand(RenameCollection);
            RemoveCollectionCommand = new RelayCommand(RemoveCollection);
            EditBoxKeyDownCommand = new RelayCommand<KeyRoutedEventArgs>(EditBoxKeyDown);
            EditBoxLostFocusCommand = new RelayCommand<RoutedEventArgs>(EditBoxLostFocus);
        }

        #endregion

        #region Command Implementation

        private async void OpenCollectionLocation()
        {
            if (!CanOpenCollection)
            {
                return;
            }

            await Launcher.LaunchFolderAsync(_innerStorageFolder);
        }

        private async void RefreshCollection()
        {
            CanvasInitialized = false;
            await InitializeInnerStorageFolder();
            await InitializeItems();
        }

        private void RenameCollection()
        {
            if (isDefault || !CanOpenCollection)
            {
                return;
            }

            EditBoxText = DisplayName;
            IsEditingName = true;
        }

        private void RemoveCollection()
        {
            OnRemoveCollectionRequestedEvent?.Invoke(this, new RemoveCollectionRequestedEventArgs(this));
        }

        private async void EditBoxKeyDown(KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Escape:
                    {
                        IsEditingName = false;

                        break;
                    }

                case VirtualKey.Enter:
                    {
                        string newName = EditBoxText;

                        CheckRenameCollectionRequestedEventArgs args = new CheckRenameCollectionRequestedEventArgs(this, newName);
                        OnCheckRenameCollectionRequestedEvent?.Invoke(this, args);

                        if (args.canRename)
                        {
                            SafeWrapperResult result = await FilesystemOperations.RenameItemAsync(_innerStorageFolder, newName, NameCollisionOption.FailIfExists);

                            if (result)
                            {
                                this._collectionFolderPath = _innerStorageFolder.Path;

                                OnPropertyChanged(nameof(DisplayName));

                                // Also update settings
                                CollectionsHelpers.UpdateSavedCollectionLocationsSetting();
                                CollectionsHelpers.UpdateLastSelectedCollectionSetting(this);
                            }
                        }

                        IsEditingName = false;

                        break;
                    }
            }
        }

        private void EditBoxLostFocus(RoutedEventArgs e)
        {
            // Cancel rename
            IsEditingName = false;
        }

        #endregion

        #region ICollectionContainerModel

        public void CheckCanOpenCollection()
        {
            if (StorageItemHelpers.Exists(_collectionFolderPath))
            {
                SetCollectionError(false);
            }
            else
            {
                SetCollectionError(true);
            }
        }

        public void DangerousSetIndex(int newIndex)
        {
            this._currentIndex = newIndex;
        }

        public IStorageFolder DangerousGetCollectionFolder()
        {
            return _innerStorageFolder;
        }

        public async Task<SafeWrapper<StorageFile>> GetEmptyFileToWrite(string extension, string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = DateTime.Now.ToString("dd.MM.yyyy HH_mm_ss");
            }

            if (_innerStorageFolder == null)
            {
                throw new UnauthorizedAccessException("The folder associated with this collection does not exist!");
            }

            string newFileName = $"{fileName}{extension}";
            SafeWrapper<StorageFile> file = await SafeWrapperRoutines.SafeWrapAsync(async () => await _innerStorageFolder.CreateFileAsync(newFileName, CreationCollisionOption.GenerateUniqueName));

            return file;
        }

        public bool HasNext()
        {
            return !IsOnNewCanvas;
        }

        public bool HasBack()
        {
            return _currentIndex > 0;
        }

        public void NavigateFirst(IPasteCanvasModel pasteCanvasModel)
        {
            _currentIndex = Items.Count;

            OnOpenNewCanvasRequestedEvent?.Invoke(this, new OpenNewCanvasRequestedEventArgs());
        }

        public async Task NavigateNext(IPasteCanvasModel pasteCanvasModel, CancellationToken cancellationToken)
        {
            _currentIndex++;

            if (_currentIndex == Items.Count)
            {
                // Open new canvas if _currentIndex exceeds the _items size
                OnOpenNewCanvasRequestedEvent?.Invoke(this, new OpenNewCanvasRequestedEventArgs());
            }
            else
            {
                // Otherwise, load existing data from file
                await LoadCanvasFromCollection(pasteCanvasModel, cancellationToken, true);
            }
        }

        public async Task NavigateLast(IPasteCanvasModel pasteCanvasModel, CancellationToken cancellationToken)
        {
            _currentIndex = 0;

            await LoadCanvasFromCollection(pasteCanvasModel, cancellationToken, false);
        }

        public async Task NavigateBack(IPasteCanvasModel pasteCanvasModel, CancellationToken cancellationToken)
        {
            _currentIndex--;

            this._currentIndex = Extensions.CollectionExtensions.IndexFitBounds(this.Items.Count, this._currentIndex);

            await LoadCanvasFromCollection(pasteCanvasModel, cancellationToken, false);
        }

        public async Task LoadCanvasFromCollection(IPasteCanvasModel pasteCanvasModel, CancellationToken cancellationToken, bool navigateNext)
        {
            // You can only load existing data
            SafeWrapperResult result = await pasteCanvasModel.TryLoadExistingData(this.Items[_currentIndex], cancellationToken);

            if (result == OperationErrorCode.NotFound) // A canvas is missing, meaning we need to reload all other items
            {
                if (!StorageItemHelpers.Exists(_innerStorageFolder?.Path))
                {
                    SetCollectionError(true);

                    // TODO: Pass error code here in the future
                    OnGoToHomePageRequestedEvent?.Invoke(this, new GoToHomePageRequestedEventArgs());
                    return;
                }

                // We must reload items because some were missing
                await InitItems("Collection is reloaded because some items were missing");

                if (navigateNext)
                {
                    _currentIndex = Extensions.CollectionExtensions.IndexFitBounds(this.Items.Count + 1, _currentIndex + 1);
                }
                else
                {
                    _currentIndex = Extensions.CollectionExtensions.IndexFitBounds(this.Items.Count + 1, _currentIndex - 1);
                }

                if (!HasNext())
                {
                    // Doesn't have next, so we're on new canvas - open new canvas
                    OnOpenNewCanvasRequestedEvent?.Invoke(this, new OpenNewCanvasRequestedEventArgs());
                }
                else
                {
                    // Load canvas again
                    result = await pasteCanvasModel.TryLoadExistingData(this.Items[_currentIndex], cancellationToken);
                }
            }
            else if ((uint)result.Details.errorCode == 0x400u)
            {
                // View Model wasn't found
                // Cannot display content for this file. - i.e. canvas display doesn't exists for this file
            }
        }

        public void RefreshAddItem(StorageFile file, BasePastedContentTypeDataModel contentType)
        {
            Items.Add(new CollectionsContainerItemViewModel(file, contentType));
        }

        #endregion

        #region Private Helpers

        private async Task InitItems(string infoText = null)
        {
            CanvasInitializing = true;
            OnCollectionItemsInitializationStartedEvent?.Invoke(this, new CollectionItemsInitializationStartedEventArgs(this, infoText));

            IsLoadingItems = true;

            IEnumerable<StorageFile> files = await this._innerStorageFolder.GetFilesAsync();

            // Sort items from oldest (last canvas) to newest (first canvas)
            files = files.OrderBy((x) => x.DateCreated.DateTime);

            // Save indexes for later
            int savedIndex = _currentIndex;
            int savedItemsCount = Items.Count;

            Items.Clear();
            foreach (var item in files)
            {
                Items.Add(new CollectionsContainerItemViewModel(item));
            }

            // TODO: save index somewhere to file?
            // Calculate new index
            int newItemsCount = Items.Count;
            int newIndex = savedIndex - (savedItemsCount - newItemsCount);
            
            this._currentIndex = Extensions.CollectionExtensions.IndexFitBounds(this.Items.Count, newIndex);

            OnCollectionItemsInitializationFinishedEvent?.Invoke(this, new CollectionItemsInitializationFinishedEventArgs(this));

            IsLoadingItems = false;
            CanvasInitializing = false;
        }

        private void SetCollectionError(bool isError)
        {
            if (isError)
            {
                ErrorIconVisibility = Visibility.Visible;
                CanOpenCollection = false;
            }
            else
            {
                ErrorIconVisibility = Visibility.Collapsed;
                CanOpenCollection = true;
            }
        }

        #endregion

        #region Public Helpers

        public async Task<bool> InitializeItems()
        {
            if (!CanvasInitialized && _innerStorageFolder != null)
            {
                CanvasInitialized = true;
                await InitItems();

                return true;
            }
            else if (CanvasInitialized)
            {
                return true;
            }

            CanOpenCollection = false;
            return false;
        }

        public async Task<bool> InitializeInnerStorageFolder()
        {
            if (_innerStorageFolder != null)
            {
                return true;
            }

            this._innerStorageFolder = await StorageItemHelpers.ToStorageItem<StorageFolder>(this._collectionFolderPath);

            if (_innerStorageFolder == null)
            {
                // Lock the collection and prevent from opening it
                SetCollectionError(true);

                return false;
            }

            return true;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            // TODO: Implement
        }

        #endregion
    }
}
