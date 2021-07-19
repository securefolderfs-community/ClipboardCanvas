using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Toolkit.Mvvm.ComponentModel;

using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.EventArguments.Collections;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.Exceptions;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.System;
using ClipboardCanvas.DataModels;

namespace ClipboardCanvas.ViewModels.UserControls.Collections
{
    public abstract class BaseCollectionViewModel : ObservableObject, ICollectionModel, IDisposable
    {
        #region Protected Members

        protected SafeWrapperResult s_CollectionFolderNotFound => new SafeWrapperResult(OperationErrorCode.NotFound, new DirectoryNotFoundException(), "The folder associated with this collection was not found.");

        protected SafeWrapperResult s_RestrictedAccessUnauthorized => StaticExceptionReporters.DefaultSafeWrapperExceptionReporter.GetStatusResult(new UnauthorizedAccessException());

        protected StorageFolder collectionFolder;

        protected CanvasNavigationDirection canvasNavigationDirection;

        protected int currentIndex;

        #endregion

        #region Public Properties

        public ObservableCollection<CollectionItemViewModel> CollectionItems { get; protected set; }

        public SearchDataModel SavedSearchData { get; set; }

        public bool IsCollectionAvailable { get; protected set; }

        public virtual bool IsOnNewCanvas => currentIndex == CollectionItems.Count;

        public CollectionItemViewModel CurrentCollectionItemViewModel => CollectionItems.Count == currentIndex ? null : CollectionItems[currentIndex];

        public string CollectionPath { get; protected set; }

        public string DisplayName => Path.GetFileName(CollectionPath);
        
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

        #endregion

        #region Events

        public event EventHandler<OpenNewCanvasRequestedEventArgs> OnOpenNewCanvasRequestedEvent;

        public event EventHandler<CanvasLoadFailedEventArgs> OnCanvasLoadFailedEvent;

        public event EventHandler<GoToHomepageRequestedEventArgs> OnGoToHomepageRequestedEvent;

        public event EventHandler<CollectionErrorRaisedEventArgs> OnCollectionErrorRaisedEvent;

        public event EventHandler<CollectionItemsInitializationStartedEventArgs> OnCollectionItemsInitializationStartedEvent;

        public event EventHandler<CollectionItemsInitializationFinishedEventArgs> OnCollectionItemsInitializationFinishedEvent;

        public event EventHandler<TipTextUpdateRequestedEventArgs> OnTipTextUpdateRequestedEvent;

        #endregion

        #region Commands

        public ICommand OpenCollectionLocationCommand { get; protected set; }

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

            // Create commands
            OpenCollectionLocationCommand = new AsyncRelayCommand(OpenCollectionLocation);
            ReloadCollectionCommand = new AsyncRelayCommand(ReloadCollection);
        }

        #endregion

        #region Command Implementation

        private async Task OpenCollectionLocation()
        {
            if (!IsCollectionAvailable)
            {
                return;
            }

            await Launcher.LaunchFolderAsync(collectionFolder);
        }

        protected async Task ReloadCollection()
        {
            await InitializeCollectionFolder();
            await InitializeCollectionItems();
        }

        #endregion

        #region ICollectionModel

        public async Task<SafeWrapper<CollectionItemViewModel>> CreateNewCollectionItemFromExtension(string extension)
        {
            string fileName = DateTime.Now.ToString("dd.mm.yyyy HH_mm_ss");
            fileName = $"{fileName}{extension}";

            return await CreateNewCollectionItemFromFilename(fileName);
        }

        public async Task<SafeWrapper<CollectionItemViewModel>> CreateNewCollectionItemFromFilename(string fileName)
        {
            if (collectionFolder == null)
            {
                return new SafeWrapper<CollectionItemViewModel>(null, s_CollectionFolderNotFound);
            }

            SafeWrapper<StorageFile> file = await FilesystemOperations.CreateFile(collectionFolder, fileName);

            var item = new CollectionItemViewModel(file.Result);
            AddCollectionItem(item);

            return new SafeWrapper<CollectionItemViewModel>(item, file.Details);
        }

        public async Task<SafeWrapperResult> DeleteCollectionItem(CollectionItemViewModel itemToDelete, bool permanently = true)
        {
            SafeWrapperResult result = await FilesystemOperations.DeleteItem(itemToDelete.Item, permanently);

            if (result)
            {
                RemoveCollectionItem(itemToDelete);
            }

            return result;
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

        public virtual void AddCollectionItem(CollectionItemViewModel collectionItemViewModel)
        {
            CollectionItems.Add(collectionItemViewModel);
        }

        public virtual void RemoveCollectionItem(CollectionItemViewModel collectionItemViewModel)
        {
            CollectionItems.Remove(collectionItemViewModel);
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

        public virtual void UpdateIndex(ICollectionItemModel collectionItemModel)
        {
            int newIndex = -1;

            if (collectionItemModel != null)
            {
                newIndex = CollectionItems.IndexOf(collectionItemModel as CollectionItemViewModel);
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

        public abstract bool CheckCollectionAvailability();

        public virtual async Task LoadCanvasFromCollection(ICanvasPreviewModel pasteCanvasModel, CancellationToken cancellationToken, CollectionItemViewModel collectionItemViewModel = null)
        {
            // You can only load existing data
            if (CollectionItems.IsEmpty() || (canvasNavigationDirection == CanvasNavigationDirection.Forward && (IsOnNewCanvas && collectionItemViewModel == null)))
            {
                OnOpenNewCanvasRequestedEvent?.Invoke(this, new OpenNewCanvasRequestedEventArgs());
                return;
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
                    if (!StorageHelpers.Exists(CollectionPath))
                    {
                        SetCollectionError(s_CollectionFolderNotFound);

                        // TODO: Pass error code here in the future
                        OnGoToHomepageRequestedEvent?.Invoke(this, new GoToHomepageRequestedEventArgs());
                        return;
                    }

                    // We must reload items because some were missing
                    OnTipTextUpdateRequestedEvent?.Invoke(this, new TipTextUpdateRequestedEventArgs("We've noticed some items went missing. We're reloading the Collection for you.", TimeSpan.FromMilliseconds(Constants.UI.CanvasContent.COLLECTION_RELOADING_TIP_DELAY)));
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
                        return;
                    }
                    else
                    {
                        int providedCollectionItemModelIndex = CollectionItems.IndexOf(collectionItemViewModel as CollectionItemViewModel);
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
                else if (result.ErrorCode == OperationErrorCode.InvalidOperation)
                {
                    // View Model wasn't found
                    // Cannot display content for this file. - i.e. canvas display doesn't exists for this file
                }

                if (!result)
                {
                    OnCanvasLoadFailedEvent?.Invoke(this, new CanvasLoadFailedEventArgs(result));
                }
            }
        }

        public virtual async Task<bool> InitializeCollectionItems()
        {
            if (collectionFolder != null)
            {
                IsCollectionInitializing = true;
                OnCollectionItemsInitializationStartedEvent?.Invoke(this, new CollectionItemsInitializationStartedEventArgs(this));

                IEnumerable<StorageFile> files = await Task.Run(async () => await collectionFolder.GetFilesAsync());

                CollectionItems.Clear();
                if (!files.IsEmpty())
                {
                    // Sort items from oldest (last canvas) to newest (first canvas)
                    files = files.OrderBy((x) => x.DateCreated.DateTime);

                    // Save indexes for later
                    int savedIndex = currentIndex;
                    int savedItemsCount = CollectionItems.Count;

                    foreach (var item in files)
                    {
                        AddCollectionItem(new CollectionItemViewModel(item));
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

        #endregion

        #region IDisposable

        public virtual void Dispose()
        {

        }

        #endregion
    }
}
