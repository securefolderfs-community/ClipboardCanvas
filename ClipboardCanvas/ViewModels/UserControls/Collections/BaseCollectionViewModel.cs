using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using System.Windows.Input;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml;

using ClipboardCanvas.GlobalizationExtensions;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.EventArguments.Collections;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.Exceptions;
using ClipboardCanvas.Contexts;
using ClipboardCanvas.Services;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.ViewModels.UserControls.InAppNotifications;
using ClipboardCanvas.Models.Autopaste;
using ClipboardCanvas.DataModels.ContentDataModels;
using ClipboardCanvas.Contexts.Operations;
using ClipboardCanvas.CanavsPasteModels;
using Microsoft.UI.Dispatching;
using Windows.System;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;
using CommunityToolkit.WinUI;

namespace ClipboardCanvas.ViewModels.UserControls.Collections
{
    public abstract class BaseCollectionViewModel : ObservableObject, ICollectionModel, IAutopasteTarget, IDisposable
    {
        #region Protected Members

        protected readonly SafeWrapperResult CollectionFolderNotFound = new SafeWrapperResult(OperationErrorCode.NotFound, new DirectoryNotFoundException(), "CollectionFolderNotFound".GetLocalized2());

        protected readonly SafeWrapperResult RestrictedAccessUnauthorized = StaticExceptionReporters.DefaultSafeWrapperExceptionReporter.GetStatusResult(new UnauthorizedAccessException());

        protected StorageFolder collectionFolder;

        protected CanvasNavigationDirection canvasNavigationDirection;

        protected int currentIndex;

        protected StorageFile iconFile;

        protected bool isFilesystemWatcherReady;

        protected FilesystemChangeWatcher2 filesystemChangeWatcher;

        protected IDialogService DialogService { get; } = Ioc.Default.GetService<IDialogService>();

        protected IUserSettingsService UserSettingsService { get; } = Ioc.Default.GetService<IUserSettingsService>();

        protected IAutopasteService AutopasteService { get; } = Ioc.Default.GetService<IAutopasteService>();

        protected IApplicationService ApplicationService { get; } = Ioc.Default.GetService<IApplicationService>();

        #endregion

        #region Public Properties

        public ObservableCollection<CollectionItemViewModel> CollectionItems { get; protected set; }

        public ICollectionModel CollectionModel => this;

        public SearchContext SearchContext { get; set; }

        public CanvasType AssociatedCanvasType { get; set; }

        public bool IsCollectionAvailable { get; protected set; }

        public virtual bool IsOnNewCanvas => currentIndex == CollectionItems.Count;

        public CollectionItemViewModel CurrentCollectionItemViewModel => CollectionItems.Count == currentIndex ? null : CollectionItems.ElementAtOrDefault(currentIndex);

        public string CollectionPath { get; protected set; }

        public virtual string DisplayName => Path.GetFileName(CollectionPath);

        public string TargetPath => CollectionPath;
        
        public bool IsCollectionInitialized { get; protected set; }

        protected bool isCanvasInitializing;
        public bool IsCollectionInitializing
        {
            get => isCanvasInitializing;
            protected set => SetProperty(ref isCanvasInitializing, value);
        }

        protected bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        protected string editBoxText;
        public string EditBoxText
        {
            get => editBoxText;
            set => SetProperty(ref editBoxText, value);
        }

        protected bool isEditingName;
        public bool IsEditingName
        {
            get => isEditingName;
            protected set => SetProperty(ref isEditingName, value);
        }

        protected bool editBoxFocus;
        public bool EditBoxFocus
        {
            get => editBoxFocus;
            protected set => SetProperty(ref editBoxFocus, value);
        }

        protected SafeWrapperResult collectionErrorInfo;
        public SafeWrapperResult CollectionErrorInfo
        {
            get => collectionErrorInfo;
            protected set => SetProperty(ref collectionErrorInfo, value);
        }

        protected bool errorIconVisibility;
        public bool ErrorIconVisibility
        {
            get => errorIconVisibility;
            protected set => SetProperty(ref errorIconVisibility, value);
        }

        protected bool usesCustomIcon;
        public bool UsesCustomIcon
        {
            get => usesCustomIcon;
            protected set => SetProperty(ref usesCustomIcon, value);
        }

        protected bool canBeSetAsAutopasteTarget;
        public bool CanBeSetAsAutopasteTarget
        {
            get => canBeSetAsAutopasteTarget;
            protected set => SetProperty(ref canBeSetAsAutopasteTarget, value);
        }

        protected BitmapImage customIcon;
        public BitmapImage CustomIcon
        {
            get => customIcon;
            protected set => SetProperty(ref customIcon, value);
        }

        #endregion

        #region Events

        public event EventHandler<CollectionOpenRequestedEventArgs> OnCollectionOpenRequestedEvent;

        public event EventHandler<OpenNewCanvasRequestedEventArgs> OnOpenNewCanvasRequestedEvent;

        public event EventHandler<CanvasLoadFailedEventArgs> OnCanvasLoadFailedEvent;

        public event EventHandler<GoToHomepageRequestedEventArgs> OnGoToHomepageRequestedEvent;

        public event EventHandler<CollectionErrorRaisedEventArgs> OnCollectionErrorRaisedEvent;

        public event EventHandler<CollectionItemsInitializationStartedEventArgs> OnCollectionItemsInitializationStartedEvent;

        public event EventHandler<CollectionItemsInitializationFinishedEventArgs> OnCollectionItemsInitializationFinishedEvent;

        public event EventHandler<TipTextUpdateRequestedEventArgs> OnTipTextUpdateRequestedEvent;

        public event EventHandler<CollectionItemAddedEventArgs> OnCollectionItemAddedEvent;

        public event EventHandler<CollectionItemRemovedEventArgs> OnCollectionItemRemovedEvent;

        public event EventHandler<CollectionItemRenamedEventArgs> OnCollectionItemRenamedEvent;

        public event EventHandler<CollectionItemContentsChangedEventArgs> OnCollectionItemContentsChangedEvent;

        #endregion

        #region Commands

        public ICommand OpenCollectionCommand { get; protected set; }

        public ICommand OpenCollectionLocationCommand { get; protected set; }

        public ICommand SetAsAutopasteTargetCommand { get; private set; }

        public ICommand ChangeCollectionIconCommand { get; protected set; }

        public ICommand RemoveCollectionIconCommand { get; protected set; }

        public ICommand ReloadCollectionCommand { get; protected set; }

        public ICommand StartRenameCollectionCommand { get; protected set; }

        public ICommand RenameBoxKeyDownCommand { get; protected set; }

        public ICommand RenameBoxLostFocusCommand { get; protected set; }

        public ICommand RemoveCollectionCommand { get; protected set; }

        #endregion

        #region Constructor

        public BaseCollectionViewModel(StorageFolder collectionFolder)
            : this(collectionFolder, null)
        {
        }

        public BaseCollectionViewModel(string collectionPath)
            : this(null, collectionPath)
        {
        }

        public BaseCollectionViewModel(StorageFolder collectionFolder, string collectionPath)
        {
            this.collectionFolder = collectionFolder;
            if (!string.IsNullOrEmpty(collectionPath))
            {
                this.CollectionPath = collectionPath;
            }
            else
            {
                this.CollectionPath = collectionFolder?.Path;
            }

            this.CollectionItems = new ObservableCollection<CollectionItemViewModel>();
            this.AssociatedCanvasType = CanvasHelpers.GetDefaultCanvasType();

            // Create commands
            OpenCollectionCommand = new RelayCommand(OpenCollection);
            OpenCollectionLocationCommand = new AsyncRelayCommand(OpenCollectionLocation);
            SetAsAutopasteTargetCommand = new RelayCommand(SetAsAutopasteTarget);
            ChangeCollectionIconCommand = new AsyncRelayCommand(ChangeCollectionIcon);
            RemoveCollectionIconCommand = new AsyncRelayCommand(RemoveCollectionIcon);
            ReloadCollectionCommand = new AsyncRelayCommand(ReloadCollection);
        }

        #endregion

        #region Command Implementation

        private void OpenCollection()
        {
            OnCollectionOpenRequestedEvent?.Invoke(this, new CollectionOpenRequestedEventArgs(this));
        }

        private async Task OpenCollectionLocation()
        {
            if (!IsCollectionAvailable)
            {
                return;
            }

            await Launcher.LaunchFolderAsync(collectionFolder);
        }

        private void SetAsAutopasteTarget()
        {
            AutopasteService.UpdateAutopasteTarget(this);
        }

        private async Task ChangeCollectionIcon()
        {
            string errorMessage = "CouldNotSetCollectionIcon".GetLocalized2();

            StorageFile pickedIcon = await DialogService.PickSingleFile(new List<string>() { ".png", ".jpg", ".jpeg"/*, ".gif", ".svg"*/ });
            if (pickedIcon != null)
            {
                // If already has an icon...
                if (UsesCustomIcon && this.iconFile != null)
                {
                    SafeWrapperResult result = await FilesystemOperations.DeleteItem(this.iconFile);

                    if (!result)
                    {
                        PushErrorNotification("CouldNotDeleteCollectionIcon".GetLocalized2(), result);
                    }
                }

                SafeWrapper<StorageFolder> iconsFolder = await StorageHelpers.GetCollectionIconsFolder();
                if (!iconsFolder)
                {
                    PushErrorNotification(errorMessage, iconsFolder);
                    return;
                }

                SafeWrapper<StorageFile> iconFile = await FilesystemOperations.CreateFile(iconsFolder, pickedIcon.Name);
                if (!iconFile)
                {
                    PushErrorNotification(errorMessage, iconFile);
                    return;
                }
                this.iconFile = iconFile.Result;

                SafeWrapperResult copyResult = await FilesystemOperations.CopyFileAsync(pickedIcon, this.iconFile, null); // TODO: In the future, add StatusCenter - StatusCenter.CreateNewOperation().OperationContext;
                if (!copyResult)
                {
                    PushErrorNotification(errorMessage, copyResult);
                    return;
                }

                SafeWrapperResult setIconResult = await InitializeIconIfSet(this.iconFile);
                if (!setIconResult)
                {
                    PushErrorNotification(errorMessage, setIconResult);
                    return;
                }

                // Serialize again because icon was updated
                SettingsSerializationHelpers.UpdateSavedCollectionsSetting();
            }
        }

        public async Task RemoveCollectionIcon()
        {
            if (UsesCustomIcon && iconFile != null)
            {
                SafeWrapperResult result = await FilesystemOperations.DeleteItem(iconFile);

                if (!result)
                {
                    if (result != OperationErrorCode.NotFound) // Only if it wasn't NotFound -- if it was, continue as usual
                    { 
                        PushErrorNotification("CouldNotRemoveCollectionIcon".GetLocalized2(), result);
                        return;
                    }
                }

                iconFile = null;
                CustomIcon = null;
                UsesCustomIcon = false;

                // Serialize again because icon was updated
                SettingsSerializationHelpers.UpdateSavedCollectionsSetting();
            }
        }

        private async Task ReloadCollection()
        {
            await InitializeCollectionFolder();
            await InitializeCollectionItems();
        }

        #endregion

        #region ICollectionModel

        public async Task<SafeWrapper<CanvasItem>> CreateNewCanvasFolder(string folderName = null)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                folderName = DateTime.Now.ToString(Constants.FileSystem.CANVAS_FILE_FILENAME_DATE_FORMAT);
            }

            SafeWrapper<StorageFolder> folder = await FilesystemOperations.CreateFolder(collectionFolder, folderName);

            CollectionItemViewModel collectionItem = null;
            if (folder)
            {
                collectionItem = new CollectionItemViewModel(folder.Result);
                AddCollectionItem(collectionItem);
            }

            return (collectionItem, folder.Details);
        }

        public async Task<SafeWrapper<CanvasItem>> CreateNewCanvasItem(string fileName)
        {
            var result = await CreateNewCollectionItem(fileName);

            return (result.Result, result.Details);
        }

        public async Task<SafeWrapper<CanvasItem>> CreateNewCanvasItemFromExtension(string extension)
        {
            var result = await CreateNewCollectionItemFromExtension(extension);

            return (result.Result, result.Details);
        }

        public async Task<SafeWrapperResult> DeleteItem(IStorageItem itemToDelete, bool permanently)
        {
            return await DeleteCollectionItem(FindCollectionItem(itemToDelete), permanently);
        }

        public async Task<SafeWrapper<CollectionItemViewModel>> CreateNewCollectionItemFromExtension(string extension)
        {
            string fileName = DateTime.Now.ToString(Constants.FileSystem.CANVAS_FILE_FILENAME_DATE_FORMAT);
            fileName = $"{fileName}{extension}";

            return await CreateNewCollectionItem(fileName);
        }

        public async Task<SafeWrapper<CollectionItemViewModel>> CreateNewCollectionItem(string fileName)
        {
            if (collectionFolder == null)
            {
                return new SafeWrapper<CollectionItemViewModel>(null, CollectionFolderNotFound);
            }

            SafeWrapper<StorageFile> file = await FilesystemOperations.CreateFile(collectionFolder, fileName);

            CollectionItemViewModel collectionItem = null;
            if (file)
            {
                collectionItem = new CollectionItemViewModel(file.Result);
                AddCollectionItem(collectionItem);
            }

            return (collectionItem, file.Details);
        }

        public async Task<SafeWrapperResult> DeleteCollectionItem(CollectionItemViewModel itemToDelete, bool permanently = true)
        {
            SafeWrapperResult result = await FilesystemOperations.DeleteItem(itemToDelete?.AssociatedItem, permanently);

            if (result)
            {
                RemoveCollectionItem(itemToDelete);
            }

            return result;
        }

        public CollectionItemViewModel FindCollectionItem(CanvasItem canvasItem)
        {
            return FindCollectionItem(canvasItem.AssociatedItem);
        }

        public CollectionItemViewModel FindCollectionItem(IStorageItem storageItem)
        {
            return FindCollectionItem(storageItem.Path);
        }

        public CollectionItemViewModel FindCollectionItem(string path)
        {
            return CollectionItems.FirstOrDefault((item) => item?.AssociatedItem.Path == path);
        }

        public virtual void NavigateFirst(ICanvasPreviewModel pasteCanvasModel)
        {
            SetIndexOnNewCanvas();
            canvasNavigationDirection = CanvasNavigationDirection.Forward;

            OnOpenNewCanvasRequestedEvent?.Invoke(this, new OpenNewCanvasRequestedEventArgs());
        }

        public virtual async Task NavigateNext(ICanvasPreviewModel pasteCanvasModel, CancellationToken cancellationToken)
        {
            currentIndex++;
            canvasNavigationDirection = CanvasNavigationDirection.Forward;

            if (currentIndex == CollectionItems.Count)
            {
                // Open new canvas if _currentIndex exceeds the _items size
                OnOpenNewCanvasRequestedEvent?.Invoke(this, new OpenNewCanvasRequestedEventArgs());
            }
            else
            {
                // Otherwise, load existing data from file
                await LoadCanvasFromCollection(pasteCanvasModel, cancellationToken);
            }
        }

        public virtual async Task NavigateLast(ICanvasPreviewModel pasteCanvasModel, CancellationToken cancellationToken)
        {
            currentIndex = 0;
            canvasNavigationDirection = CanvasNavigationDirection.Backward;

            await LoadCanvasFromCollection(pasteCanvasModel, cancellationToken);
        }

        public virtual async Task NavigateBack(ICanvasPreviewModel pasteCanvasModel, CancellationToken cancellationToken)
        {
            currentIndex--;
            canvasNavigationDirection = CanvasNavigationDirection.Backward;

            await LoadCanvasFromCollection(pasteCanvasModel, cancellationToken);
        }

        public virtual void AddCollectionItem(CollectionItemViewModel collectionItemViewModel, bool fromInit = false)
        {
            if (!fromInit)
            {
                OnCollectionItemAddedEvent?.Invoke(this, new CollectionItemAddedEventArgs(this, collectionItemViewModel));
            }
            CollectionItems.Add(collectionItemViewModel);
        }

        public virtual void RemoveCollectionItem(CollectionItemViewModel collectionItemViewModel)
        {
            if (collectionItemViewModel != null)
            {
                OnCollectionItemRemovedEvent?.Invoke(this, new CollectionItemRemovedEventArgs(this, collectionItemViewModel));
                CollectionItems.Remove(collectionItemViewModel);
            }
        }

        public virtual bool HasNext()
        {
            return currentIndex < CollectionItems.Count;
        }

        public virtual bool HasBack()
        {
            return currentIndex > 0;
        }

        public virtual void SetIndexOnNewCanvas()
        {
            currentIndex = CollectionItems.Count;
        }

        public virtual void UpdateIndex(CollectionItemViewModel collectionItemViewModel)
        {
            int newIndex = -1;

            if (collectionItemViewModel != null)
            {
                newIndex = CollectionItems.IndexOf(collectionItemViewModel);
            }

            if (newIndex == -1)
            {
                SetIndexOnNewCanvas();
            }
            else
            {
                currentIndex = newIndex;
            }
        }

        public bool IsOnOpenedCanvas(CollectionItemViewModel collectionItemViewModel)
        {
            int indexOfRequestedItemViewModel = CollectionItems.IndexOf(collectionItemViewModel);

            return indexOfRequestedItemViewModel == currentIndex;
        }

        public abstract bool CheckCollectionAvailability();

        public virtual CollectionConfigurationModel ConstructConfigurationModel()
        {
            return new CollectionConfigurationModel(CollectionPath, UsesCustomIcon, iconFile?.Name);
        }

        public virtual async Task<SafeWrapperResult> LoadCanvasFromCollection(ICanvasPreviewModel pasteCanvasModel, CancellationToken cancellationToken, CollectionItemViewModel collectionItemViewModel = null)
        {
            // You can only load existing data
            if (CollectionItems.IsEmpty() || (canvasNavigationDirection == CanvasNavigationDirection.Forward && (IsOnNewCanvas && collectionItemViewModel == null)))
            {
                OnOpenNewCanvasRequestedEvent?.Invoke(this, new OpenNewCanvasRequestedEventArgs());
                return SafeWrapperResult.SUCCESS;
            }
            else
            {
                currentIndex = Extensions.CollectionExtensions.IndexFitBounds(CollectionItems.Count, currentIndex);

                if (collectionItemViewModel == null)
                {
                    collectionItemViewModel = CollectionItems[currentIndex];
                }
                else
                {
                    int providedCollectionItemModelIndex = CollectionItems.IndexOf(collectionItemViewModel);
                    currentIndex = providedCollectionItemModelIndex;
                }

                SafeWrapperResult result = await pasteCanvasModel.TryLoadExistingData(collectionItemViewModel, cancellationToken);

                if (result == OperationErrorCode.NotFound && result.Exception is not ReferencedFileNotFoundException) // A canvas is missing, meaning we need to reload all other items
                {
                    if (!StorageHelpers.Existsh(CollectionPath))
                    {
                        SetCollectionError(CollectionFolderNotFound);

                        // TODO: Pass error code here in the future
                        OnGoToHomepageRequestedEvent?.Invoke(this, new GoToHomepageRequestedEventArgs());
                        return result;
                    }

                    // We must reload items because some were missing
                    OnTipTextUpdateRequestedEvent?.Invoke(this, new TipTextUpdateRequestedEventArgs("CollectionItemsMissing".GetLocalized2(), TimeSpan.FromMilliseconds(Constants.UI.CanvasContent.COLLECTION_RELOADING_TIP_DELAY)));
                    await InitializeCollectionItems();

                    if (canvasNavigationDirection == CanvasNavigationDirection.Forward)
                    {
                        if (!HasNext())
                        {
                            // Doesn't have next, so we're on new canvas - open new canvas
                            OnOpenNewCanvasRequestedEvent?.Invoke(this, new OpenNewCanvasRequestedEventArgs());
                        }
                        else
                        {
                            currentIndex++;
                        }
                    }
                    else
                    {
                        if (HasBack())
                        {
                            currentIndex--;
                        }
                    }

                    if (CollectionItems.IsEmpty())
                    {
                        OnOpenNewCanvasRequestedEvent?.Invoke(this, new OpenNewCanvasRequestedEventArgs());
                        return SafeWrapperResult.SUCCESS;
                    }
                    else
                    {
                        int providedCollectionItemModelIndex = CollectionItems.IndexOf(collectionItemViewModel);
                        if (providedCollectionItemModelIndex != -1)
                        {
                            currentIndex = providedCollectionItemModelIndex;
                        }

                        if (currentIndex < CollectionItems.Count)
                        {
                            collectionItemViewModel = CollectionItems[currentIndex];

                            // Load canvas again
                            result = await pasteCanvasModel.TryLoadExistingData(collectionItemViewModel, cancellationToken);
                        }
                    }
                }
                else if (result == OperationErrorCode.InProgress)
                {
                    // Content is still being pasted...
                    // TODO: Hook event to collectionItemViewModel.OperationContext.OnOperationFinishedEvent
                }
                else if (result == OperationErrorCode.InvalidOperation)
                {
                    // View Model wasn't found
                    // Cannot display content for this file. - e.g. canvas display doesn't exists for this file
                }

                if (!result)
                {
                    OnCanvasLoadFailedEvent?.Invoke(this, new CanvasLoadFailedEventArgs(result));
                }

                return result;
            }
        }

        public virtual Task SetupFilesystemWatcher()
        {
            if (!isFilesystemWatcherReady)
            {
                isFilesystemWatcherReady = true;

                this.filesystemChangeWatcher = new(collectionFolder.Path);
                this.filesystemChangeWatcher.OnChangeRegisteredEvent += FilesystemChangeWatcher_OnChangeRegisteredEvent;
            }

            return Task.CompletedTask;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility",
            Justification = "<Pending>")]
        private async void FilesystemChangeWatcher_OnChangeRegisteredEvent(object sender, ChangeRegisteredEventArgs2 e)
        {
            // Issue: items are pasted added to collection twice
            return;
            try
            {
                await MainWindow.Instance.DispatcherQueue.EnqueueAsync(async () =>
                {
                    if (e.FullPath.EndsWith(".TMP") || e.FullPath.EndsWith("~tmp"))
                        return;

                    // Reflect changes in collection
                    string itemParentFolder = Path.GetDirectoryName(e.FullPath);
                    string watchedParentFolder = collectionFolder.Path;
                    if (itemParentFolder != watchedParentFolder)
                        return;

                    IStorageItem changedItem = await StorageHelpers.ToStorageItem<IStorageItem>(e.FullPath);

                    switch (e.ChangeType)
                    {
                        case WatcherChangeTypes.Created:
                        {
                            // Add new collection item
                            if (changedItem != null && !CollectionItems.Any((i) => e.FullPath == i.AssociatedItem.Path))
                            {
                                var collectionItem = new CollectionItemViewModel(changedItem);
                                AddCollectionItem(collectionItem);
                            }

                            break;
                        }

                        case WatcherChangeTypes.Deleted:
                        {
                            // Remove the collection item
                            var collectionItem = FindCollectionItem(e.FullPath);
                            RemoveCollectionItem(collectionItem);
                            break;
                        }

                        case WatcherChangeTypes.Renamed:
                        {
                            if (changedItem != null)
                            {
                                string oldName = Path.GetFileName(e.OldPath);
                                string newName = Path.GetFileName(e.FullPath);

                                string oldParentPath = Path.GetDirectoryName(e.OldPath);
                                string newParentPath = Path.GetDirectoryName(e.FullPath);

                                if ((oldName != newName) && (oldParentPath == newParentPath))
                                {
                                    // Renamed
                                    var collectionItem = FindCollectionItem(e.OldPath);
                                    if (collectionItem is null)
                                        break;

                                    collectionItem.DangerousUpdateItem(changedItem);
                                    OnCollectionItemRenamedEvent?.Invoke(this,
                                        new CollectionItemRenamedEventArgs(this, collectionItem,
                                            e.OldPath));
                                }
                            }

                            break;
                        }

                        case WatcherChangeTypes.Changed:
                        {
                            var collectionItem = FindCollectionItem(changedItem?.Path);
                            if (collectionItem != null)
                            {
                                OnCollectionItemContentsChangedEvent?.Invoke(this,
                                    new CollectionItemContentsChangedEventArgs(this, collectionItem));
                            }

                            break;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                // Wrong thread exception bug?
            }
        }

        public virtual async Task<bool> InitializeCollectionItems()
        {
            if (collectionFolder != null)
            {
                IsCollectionInitializing = true;
                OnCollectionItemsInitializationStartedEvent?.Invoke(this, new CollectionItemsInitializationStartedEventArgs(this));

                IEnumerable<IStorageItem> items = await Task.Run(async () => await collectionFolder.GetItemsAsync());

                await SetupFilesystemWatcher();

                CollectionItems.Clear();
                if (!items.IsEmpty())
                {
                    // Sort items from oldest (last canvas) to newest (first canvas)
                    items = items.OrderBy((x) => x.DateCreated.DateTime);

                    // Save indexes for later
                    int savedIndex = currentIndex;
                    int savedItemsCount = CollectionItems.Count;

                    foreach (var item in items)
                    {
                        AddCollectionItem(new CollectionItemViewModel(item), true);
                    }

                    // TODO: save index somewhere to file?
                    // Calculate new index
                    int newItemsCount = CollectionItems.Count;
                    int newIndex = Math.Max(savedIndex, savedIndex - (savedItemsCount - newItemsCount));

                    this.currentIndex = Extensions.CollectionExtensions.IndexFitBounds(this.CollectionItems.Count + 1, newIndex); // Increase the Items.Count by one to account for new canvas being always Items.Count
                }

                IsCollectionInitializing = false;
                IsCollectionInitialized = true;
                OnCollectionItemsInitializationFinishedEvent?.Invoke(this, new CollectionItemsInitializationFinishedEventArgs(this));

                return true;
            }

            IsCollectionInitialized = false;
            return false;
        }

        public async Task<bool> InitializeCollectionFolder()
        {
            if (!CheckCollectionAvailability())
            {
                return false;
            }
            else
            {
                SafeWrapper<StorageFolder> result = await StorageHelpers.ToStorageItemWithError<StorageFolder>(CollectionPath);
                collectionFolder = result;

                if (!result)
                {
                    SetCollectionError(result);

                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public async Task<SafeWrapperResult> InitializeIconIfSet(CollectionConfigurationModel collectionConfiguration)
        {
            if (collectionConfiguration == null || !collectionConfiguration.usesCustomIcon)
            {
                return SafeWrapperResult.SUCCESS;
            }

            SafeWrapper<StorageFolder> iconsFolder = await StorageHelpers.GetCollectionIconsFolder();
            if (!iconsFolder)
            {
                return iconsFolder;
            }

            SafeWrapper<StorageFile> iconFile = await StorageHelpers.ToStorageItemWithError<StorageFile>(Path.Combine(iconsFolder.Result.Path, collectionConfiguration.iconFileName));

            if (!iconFile)
            {
                return iconFile;
            }

            return await InitializeIconIfSet(iconFile);
        }

        public async Task<SafeWrapperResult> InitializeIconIfSet(StorageFile iconFile)
        {
            UsesCustomIcon = true;
            this.iconFile = iconFile;
            await Task.Delay(Constants.UI.CONTROL_LOAD_DELAY);

            return await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                using (IRandomAccessStream fileStream = await iconFile.OpenReadAsync())
                {
                    CustomIcon = await ImagingHelpers.ToBitmapAsync(fileStream);
                }
            });
        }

        #endregion

        #region IAutopasteTarget

        public async Task<SafeWrapper<CanvasItem>> PasteData(DataPackageView dataPackage, CancellationToken cancellationToken)
        {
            BaseContentTypeModel pastedItemContentType = await BaseContentTypeModel.GetContentTypeFromDataPackage(dataPackage);

            if (pastedItemContentType is InvalidContentTypeDataModel invalidContentType)
            {
                return (null, invalidContentType.error);
            }

            // Get correct IPasteModel from contentType
            using IPasteModel canvasPasteModel = CanvasHelpers.GetPasteModelFromContentType(pastedItemContentType, this, new StatusCenterOperationReceiver());
            SafeWrapper<CanvasItem> canvasItemResult = await canvasPasteModel.PasteData(dataPackage, UserSettingsService.AlwaysPasteFilesAsReference, cancellationToken);
            if (canvasItemResult && currentIndex == CollectionItems.Count - 1) // Was on new canvas, item was pasted and is now one behind - set to new canvas
            {
                SetIndexOnNewCanvas();
            }

            return canvasItemResult;
        }

        #endregion

        #region Protected Helpers

        protected virtual void SetCollectionError(SafeWrapperResult safeWrapperResult)
        {
            if (!safeWrapperResult)
            {
                CollectionErrorInfo = safeWrapperResult;
                ErrorIconVisibility = true;
                IsCollectionAvailable = false;
            }
            else
            {
                ErrorIconVisibility = false;
                IsCollectionAvailable = true;
            }

            OnCollectionErrorRaisedEvent?.Invoke(this, new CollectionErrorRaisedEventArgs(safeWrapperResult));
        }

        protected virtual void PushErrorNotification(string errorMessage, SafeWrapperResult result)
        {
            IInAppNotification notification = DialogService.GetNotification();
            notification.ViewModel.NotificationText = string.Format("CollectionErrorNotificationDescription".GetLocalized2(), errorMessage, result.ErrorCode);
            notification.ViewModel.ShownButtons = InAppNotificationButtonType.OkButton;

            notification.Show(Constants.UI.Notifications.NOTIFICATION_DEFAULT_SHOW_TIME);
        }

        #endregion

        #region IDisposable

        public virtual void Dispose()
        {
            if (isFilesystemWatcherReady)
            {
                filesystemChangeWatcher.OnChangeRegisteredEvent -= FilesystemChangeWatcher_OnChangeRegisteredEvent;
                filesystemChangeWatcher.Dispose();
            }
        }

        #endregion
    }
}
