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
using ClipboardCanvas.Helpers.FilesystemHelpers;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.ReferenceItems;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class CollectionsContainerViewModel : ObservableObject, ICollectionsContainerModel, IDisposable
    {
        #region Private Members

        private int _currentIndex;

        private bool _itemsInitialized;

        #endregion

        #region Public Members

        public readonly bool isDefault;

        public string collectionFolderPath;

        #endregion

        #region Public Properties

        public List<CollectionsContainerItemModel> Items { get; private set; }

        public string Name
        {
            get => Path.GetFileName(collectionFolderPath);
        }

        public string DisplayName
        {
            get => Path.GetFileName(collectionFolderPath);
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

        public IStorageFolder InnerStorageFolder { get; private set; }

        public bool IsFilled => _currentIndex < Items.Count;

        public ICollectionsContainerItemModel CurrentCanvas => this.Items[_currentIndex];

        #endregion

        #region Events

        public event EventHandler<ItemsRefreshRequestedEventArgs> OnItemsRefreshRequestedEvent;

        public event EventHandler<RenameCollectionRequestedEventArgs> OnRenameCollectionRequestedEvent;

        public event EventHandler<RemoveCollectionRequestedEventArgs> OnRemoveCollectionRequestedEvent;

        #endregion

        #region Commands

        public ICommand OpenCollectionLocationCommand { get; private set; }

        public ICommand RenameCollectionCommand { get; private set; }

        public ICommand RemoveCollectionCommand { get; private set; }

        public ICommand EditBoxKeyDownCommand { get; private set; }

        public ICommand EditBoxLostFocusCommand { get; private set; }

        #endregion

        #region Constructor

        public CollectionsContainerViewModel(string collectionFolderPath, bool isDefault = false)
        {
            this.collectionFolderPath = collectionFolderPath;
            this.isDefault = isDefault;
            OnPropertyChanged(nameof(DisplayName));
            this.Items = new List<CollectionsContainerItemModel>();

            // Create commands
            OpenCollectionLocationCommand = new RelayCommand(OpenCollectionLocation);
            RenameCollectionCommand = new RelayCommand(RenameCollection);
            RemoveCollectionCommand = new RelayCommand(RemoveCollection);
            EditBoxKeyDownCommand = new RelayCommand<KeyRoutedEventArgs>(EditBoxKeyDown);
            EditBoxLostFocusCommand = new RelayCommand<RoutedEventArgs>(EditBoxLostFocus);

            Initialize();
        }

        #endregion

        #region Command Implementation

        private async void OpenCollectionLocation()
        {
            await Launcher.LaunchFolderAsync(InnerStorageFolder);
        }

        private void RenameCollection()
        {
            if (isDefault)
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

        private void EditBoxKeyDown(KeyRoutedEventArgs e)
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
                        string oldPath = collectionFolderPath;

                        RenameCollectionRequestedEventArgs args = new RenameCollectionRequestedEventArgs(this, EditBoxText);
                        OnRenameCollectionRequestedEvent?.Invoke(this, args);

                        if (args.renamed)
                        {
                            this.collectionFolderPath = InnerStorageFolder.Path;
                            OnPropertyChanged(nameof(DisplayName));

                            // Also update settings
                            CollectionsHelpers.UpdateSavedCollectionLocationsSetting();
                            CollectionsHelpers.UpdateLastSelectedCollectionSetting(this);
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

        public void DangerousSetIndex(int newIndex)
        {
            this._currentIndex = newIndex;
        }

        public async Task<SafeWrapper<StorageFile>> GetEmptyFileToWrite(string extension, string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = DateTime.Now.ToString("dd.MM.yyyy HH_mm_ss");
            }

            if (InnerStorageFolder == null)
            {
                throw new UnauthorizedAccessException("The folder associated with this collection does not exist!");
            }

            string newFileName = $"{fileName}{extension}";
            SafeWrapper<StorageFile> file = await SafeWrapperRoutines.SafeWrapAsync(() => InnerStorageFolder.CreateFileAsync(newFileName, CreationCollisionOption.GenerateUniqueName).AsTask());

            return file;
        }

        public bool HasNext()
        {
            return _currentIndex <= (Items.Count - 1); // Return true if there's any item available or _currentIndex is on last filled canvas
        }

        public bool HasBack()
        {
            return _currentIndex > 0;
        }

        public void NavigateFirst(IPasteCanvasModel pasteCanvasModel)
        {
            _currentIndex = Items.Count;

            pasteCanvasModel.OpenNewCanvas();
        }

        public async Task NavigateNext(IPasteCanvasModel pasteCanvasModel, CancellationToken cancellationToken)
        {
            _currentIndex++;

            if (_currentIndex == Items.Count)
            {
                // Open new canvas if _currentIndex exceeds the _items size
                pasteCanvasModel.OpenNewCanvas();
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
        // TODO: Should isOperationSwitchNextCanvas parameter be removed?
        {
            //Debugger.Break(); // TODO: Investigate high memory usage on load

            // You can only load existing data
            SafeWrapperResult result = await pasteCanvasModel.TryLoadExistingData(this.Items[_currentIndex], cancellationToken);

            if (result == OperationErrorCode.NotFound) // A canvas is missing, meaning we need to reload all other items
            {
                // Save indexes for later
                int savedIndex = _currentIndex;
                int savedItemsCount = Items.Count;

                // We must reload items because some were missing
                await InitItems();

                // Calculate new index
                int newItemsCount = Items.Count;
                int newIndex = savedIndex - (savedItemsCount - newItemsCount);
                if (navigateNext)
                {
                    newIndex += savedItemsCount - newItemsCount;
                }

                this._currentIndex = Extensions.CollectionExtensions.IndexFitBounds(this.Items.Count, newIndex);

                if (!HasNext())
                {
                    // Doesn't have next, so we're on new canvas - open new canvas
                    pasteCanvasModel.OpenNewCanvas();
                }
                else
                {
                    // Load canvas again
                    result = await pasteCanvasModel.TryLoadExistingData(this.Items[_currentIndex], cancellationToken);
                }
            }
            else if (result == OperationErrorCode.Unauthorized)
            {
                // View Model wasn't found
                // Cannot display content for this file. - i.e. canvas display doesn't exists for this file
                // TODO: Handle this error
            }
        }

        public void RefreshAddItem(StorageFile file, BasePastedContentTypeDataModel contentType)
        {
            Items.Add(new CollectionsContainerItemModel(file, contentType));
        }

        #endregion

        #region Private Helpers

        private async void Initialize()
        {
            if (await InitInnerStorageFolder())
            {
                await InitItems();
            }
            else
            {
                // TODO: Display exclamation mark icon that something went wrong
            }
        }

        private async Task<bool> InitInnerStorageFolder()
        {
            if (InnerStorageFolder != null)
            {
                return true;
            }

            this.InnerStorageFolder = await StorageItemHelpers.ToStorageItem<IStorageFolder>(this.collectionFolderPath);

            if (InnerStorageFolder == null)
            {
                // Looks like the collection has been moved/deleted, remove it!
                OnRemoveCollectionRequestedEvent?.Invoke(this, new RemoveCollectionRequestedEventArgs(this));
                return false;
            }

            return true;
        }

        private async Task InitItems()
        {
            IEnumerable<StorageFile> files = await this.InnerStorageFolder.GetFilesAsync();

            // Sort items from oldest (last canvas) to newest (first canvas)
            files = files.OrderBy((x) => x.DateCreated.DateTime);

            Items.Clear();
            foreach (var item in files)
            {
                Items.Add(new CollectionsContainerItemModel(item));
            }

            // TODO: save index somewhere to file?
            _currentIndex = Items.Count; // - exceeds possible index range because we also want to be on unfilled canvas

            OnItemsRefreshRequestedEvent?.Invoke(this, new ItemsRefreshRequestedEventArgs(this));
        }

        public async Task<bool> InitializeItems()
        {
            if (!_itemsInitialized && InnerStorageFolder != null)
            {
                _itemsInitialized = true;
                await InitItems();

                return true;
            }
            else if (_itemsInitialized)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Public Helpers

        public void InitInnerStorageFolder(IStorageFolder folder)
        {
            this.InnerStorageFolder = folder;

            if (InnerStorageFolder == null)
            {
                Debugger.Break(); // bruh :((
            }
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
