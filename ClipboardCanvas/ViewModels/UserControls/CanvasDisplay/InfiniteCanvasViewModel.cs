using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Toolkit.Mvvm.Input;
using System.Linq;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Microsoft.Toolkit.Uwp;
using Windows.Storage.Streams;

using ClipboardCanvas.CanavsPasteModels;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.EventArguments.InfiniteCanvasEventArgs;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class InfiniteCanvasViewModel : BaseCanvasViewModel
    {
        private InfiniteCanvasItem InfiniteCanvasItem => canvasItem as InfiniteCanvasItem;

        private FilesystemChangeWatcher _filesystemChangeWatcher;

        private bool _isFilesystemWatcherReady;

        private ICanvasItemReceiverModel _infiniteCanvasFileReceiver;

        private IInteractableCanvasControlModel InteractableCanvasControlModel => ControlView?.InteractableCanvasModel;

        private IInfiniteCanvasControlView _ControlView;
        public IInfiniteCanvasControlView ControlView
        {
            get => _ControlView;
            set
            {
                if (_ControlView != null)
                {
                    _ControlView.InteractableCanvasModel.OnInfiniteCanvasSaveRequestedEvent -= InteractableCanvasModel_OnInfiniteCanvasSaveRequestedEvent;
                }

                _ControlView = value;

                if (_ControlView != null)
                {
                    _ControlView.InteractableCanvasModel.OnInfiniteCanvasSaveRequestedEvent += InteractableCanvasModel_OnInfiniteCanvasSaveRequestedEvent;
                }
            }
        }

        #region Constructor

        public InfiniteCanvasViewModel(IBaseCanvasPreviewControlView view, BaseContentTypeModel contentType)
            : base (StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, contentType, view)
        {
        }

        #endregion

        #region Override

        public override async Task<SafeWrapperResult> TryPasteData(DataPackageView dataPackage, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;

            RaiseOnPasteInitiatedEvent(this, new PasteInitiatedEventArgs(false, null, associatedCollection));

            // First, set Infinite Canvas folder
            SafeWrapperResult initializeInfiniteCanvasFolderResult = await InitializeInfiniteCanvasFolder();
            if (!AssertNoError(initializeInfiniteCanvasFolderResult))
            {
                return initializeInfiniteCanvasFolderResult;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            // Get content type from data package
            BaseContentTypeModel contentType = await BaseContentTypeModel.GetContentTypeFromDataPackage(dataPackage);

            // Get correct IPasteModel from contentType
            IPasteModel canvasPasteModel = CanvasHelpers.GetPasteModelFromContentType(contentType, _infiniteCanvasFileReceiver, null);

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            if (canvasPasteModel == null)
            {
                return BaseContentTypeModel.CannotDisplayContentForTypeResult;
            }

            // Paste data
            SafeWrapper<CanvasItem> pastedFile = await canvasPasteModel.PasteData(dataPackage, UserSettings.AlwaysPasteFilesAsReference, cancellationToken);

            // We don't need IPasteModel anymore, so dispose it
            canvasPasteModel.Dispose();

            if (!pastedFile)
            {
                return pastedFile;
            }

            // Add new object to Infinite Canvas
            var interactableCanvasControlItem = await InteractableCanvasControlModel.AddItem(associatedCollection, contentType, pastedFile, _infiniteCanvasFileReceiver, cancellationToken);

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            // Wait for control to load
            await Task.Delay(Constants.UI.CONTROL_LOAD_DELAY);

            // Update item position based on datapackage
            InteractableCanvasControlModel.UpdateItemPositionFromDataPackage(dataPackage, interactableCanvasControlItem);

            // Save data after pasting
            SafeWrapperResult saveDataResult = await SaveConfigurationModel();

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            AssertNoError(saveDataResult); // Only for notification

            // Fetch data to view
            SafeWrapperResult fetchDataToViewResult = await interactableCanvasControlItem.LoadContent();

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            // Start filesystem change tracker
            await StartFilesystemChangeWatcher((await InfiniteCanvasItem.SourceItem) as StorageFolder);

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            RaiseOnContentLoadedEvent(this, new ContentLoadedEventArgs(contentType, false, false, CanPasteReference, true));

            return fetchDataToViewResult;
        }

        public override async Task<SafeWrapperResult> TryLoadExistingData(CanvasItem canvasItem, BaseContentTypeModel contentType, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            this.canvasItem = new InfiniteCanvasItem(canvasItem.AssociatedItem, await canvasItem.SourceItem);
            this.contentType = contentType;

            if (IsDisposed)
            {
                return null;
            }

            RaiseOnContentStartedLoadingEvent(this, new ContentStartedLoadingEventArgs(contentType));

            // First, initialize Infinite Canvas folder
            SafeWrapperResult initializeInfiniteCanvasFolderResult = await InitializeInfiniteCanvasFolder();
            if (!AssertNoError(initializeInfiniteCanvasFolderResult))
            {
                return initializeInfiniteCanvasFolderResult;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            // Get all items in Infinite Canvas folder
            IEnumerable<IStorageItem> items = await Task.Run(async () => await ((await canvasItem.SourceItem) as StorageFolder).GetItemsAsync());

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            List<Task> loadContentTasks = new List<Task>();
            List<InteractableCanvasControlItemViewModel> interactableCanvasList = new List<InteractableCanvasControlItemViewModel>();

            foreach (var item in items)
            {
                if (FilesystemHelpers.IsPathEqualExtension(item.Path, Constants.FileSystem.INFINITE_CANVAS_CONFIGURATION_FILE_EXTENSION)
                    || Path.GetFileName(item.Path) == Constants.FileSystem.INFINITE_CANVAS_PREVIEW_IMAGE_FILENAME)
                {
                    continue;
                }

                if (cancellationToken.IsCancellationRequested) // Check if it's canceled
                {
                    DiscardData();
                    return SafeWrapperResult.CANCEL;
                }

                // Initialize parameters
                BaseContentTypeModel itemContentType = await BaseContentTypeModel.GetContentType(item, null);
                CanvasItem itemCanvasItem = new CanvasItem(item);

                if (IsDisposed || InteractableCanvasControlModel == null)
                {
                    return null;
                }

                if (cancellationToken.IsCancellationRequested) // Check if it's canceled
                {
                    DiscardData();
                    return SafeWrapperResult.CANCEL;
                }

                // Add to canvas
                var interactableCanvasItem = await InteractableCanvasControlModel?.AddItem(associatedCollection, itemContentType, itemCanvasItem, _infiniteCanvasFileReceiver, cancellationToken);
                interactableCanvasList.Add(interactableCanvasItem);
            }

            // Read Infinite Canvas configuration file
            SafeWrapper<string> readFile = await FilesystemOperations.ReadFileText(InfiniteCanvasItem.ConfigurationFile);
            if (!AssertNoError(readFile))
            {
                return readFile;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            // Set configuration model
            var canvasConfigurationModel = JsonConvert.DeserializeObject<InfiniteCanvasConfigurationModel>(readFile);
            InteractableCanvasControlModel.SetConfigurationModel(canvasConfigurationModel);

            // Load item previews
            foreach (var item in interactableCanvasList)
            {
                loadContentTasks.Add(item.LoadContent(true));
            }
            await Task.WhenAll(loadContentTasks);

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            // Start filesystem change tracker
            await StartFilesystemChangeWatcher((await InfiniteCanvasItem.SourceItem) as StorageFolder);

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            // Always regenerate canvas preview on load to update it
            await InteractableCanvasControlModel.RegenerateCanvasPreview();

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            RaiseOnContentLoadedEvent(this, new ContentLoadedEventArgs(contentType, IsContentLoaded, isContentAsReference, CanPasteReference, true));

            return SafeWrapperResult.SUCCESS;
        }

        private async Task<SafeWrapperResult> SaveConfigurationModel()
        {
            InfiniteCanvasConfigurationModel canvasConfigurationModel = InteractableCanvasControlModel.ConstructConfigurationModel();

            string serializedConfig = JsonConvert.SerializeObject(canvasConfigurationModel, Formatting.Indented);
            SafeWrapperResult writeConfigResult = await FilesystemOperations.WriteFileText(InfiniteCanvasItem.ConfigurationFile, serializedConfig);

            return writeConfigResult;
        }

        protected override Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
        {
            return Task.FromResult(SafeWrapperResult.CANCEL);
        }

        protected override Task<SafeWrapperResult> TryFetchDataToView()
        {
            return Task.FromResult(SafeWrapperResult.CANCEL);
        }

        public override async Task<IEnumerable<SuggestedActionsControlItemViewModel>> GetSuggestedActions()
        {
            List<SuggestedActionsControlItemViewModel> actions = (await base.GetSuggestedActions()).ToList();

            if (associatedItem == null)
            {
                return actions;
            }

            // Remove open file
            actions.RemoveFront();

            // Add open Infinite Canvas folder
            var action_openInfiniteCanvasFolder = new SuggestedActionsControlItemViewModel(
                new AsyncRelayCommand(async () =>
                {
                    await associatedCollection.CurrentCollectionItemViewModel.OpenFile();
                }), "Open Infinite Canvas folder", "\uE838");

            actions.AddFront(action_openInfiniteCanvasFolder);

            return actions;
        }

        protected override IPasteModel SetCanvasPasteModel()
        {
            return null;
        }

        #endregion

        #region Event Handlers

        private async void InteractableCanvasModel_OnInfiniteCanvasSaveRequestedEvent(object sender, InfiniteCanvasSaveRequestedEventArgs e)
        {
            // Save configuration
            SafeWrapperResult saveDataResult = await SaveConfigurationModel();
            AssertNoError(saveDataResult); // Only for notification

            // Save canvas image preview
            SafeWrapperResult imagePreviewSaveResult = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                using (e.canvasImageStream)
                {
                    using (IRandomAccessStream fileStream = await InfiniteCanvasItem.CanvasPreviewImageFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await e.canvasImageStream.AsStreamForRead().CopyToAsync(fileStream.AsStreamForWrite());
                    }
                }
            });
        }

        private async void FilesystemChangeWatcher_OnChangeRegisteredEvent(object sender, ChangeRegisteredEventArgs e)
        {
            // Get changes
            IEnumerable<StorageLibraryChange> changes = await e.filesystemChangeReader.ReadBatchAsync();

            // Accept changes
            await e.filesystemChangeReader.AcceptChangesAsync();

            // Reflect changes
            foreach (var item in changes)
            {
                if (FilesystemHelpers.IsPathEqualExtension(item.Path, Constants.FileSystem.INFINITE_CANVAS_CONFIGURATION_FILE_EXTENSION)
                    || Path.GetFileName(item.Path) == Constants.FileSystem.INFINITE_CANVAS_PREVIEW_IMAGE_FILENAME)
                {
                    continue;
                }

                string itemParentFolder = Path.GetDirectoryName(item.Path);
                string watchedParentFolder = Path.GetDirectoryName((await InfiniteCanvasItem.SourceItem).Path);
                if (itemParentFolder != watchedParentFolder)
                {
                    continue;
                }

                IStorageItem changedItem = await item.GetStorageItemAsync();

                switch (item.ChangeType)
                {
                    case StorageLibraryChangeType.ChangeTrackingLost:
                        {
                            e.filesystemChangeTracker.Reset();
                            break;
                        }

                    case StorageLibraryChangeType.Created:
                        {
                            await CoreApplication.MainView.DispatcherQueue.EnqueueAsync(async () =>
                            {
                                if (InteractableCanvasControlModel.ContainsItem(InteractableCanvasControlModel.FindItem(changedItem.Path)))
                                {
                                    return;
                                }

                                BaseContentTypeModel contentType = await BaseContentTypeModel.GetContentType(changedItem, null);
                                if (contentType != null)
                                {
                                    CanvasItem canvasItem = new CanvasItem(changedItem);

                                    var interactableCanvasControlItem = await InteractableCanvasControlModel.AddItem(associatedCollection, contentType, canvasItem, _infiniteCanvasFileReceiver, cancellationToken);
                                    if (interactableCanvasControlItem != null)
                                    {
                                        await interactableCanvasControlItem.LoadContent();
                                    }
                                }
                            });
                            break;
                        }

                    case StorageLibraryChangeType.MovedOutOfLibrary:
                    case StorageLibraryChangeType.Deleted:
                        {
                            await CoreApplication.MainView.DispatcherQueue.EnqueueAsync(() =>
                            {
                                InteractableCanvasControlModel.RemoveItem(InteractableCanvasControlModel.FindItem(item.Path));
                            });
                            break;
                        }

                    case StorageLibraryChangeType.MovedOrRenamed:
                        {
                            await CoreApplication.MainView.DispatcherQueue.EnqueueAsync(async () =>
                            {
                                string oldName = Path.GetFileName(item.PreviousPath);
                                string newName = Path.GetFileName(item.Path);

                                string oldParentPath = Path.GetDirectoryName(item.PreviousPath);
                                string newParentPath = Path.GetDirectoryName(item.Path);

                                if ((oldName != newName) && (oldParentPath == newParentPath))
                                {
                                    // Renamed
                                    var interactableCanvasControlItem = InteractableCanvasControlModel.FindItem(item.PreviousPath);

                                    interactableCanvasControlItem.CanvasItem.DangerousUpdateItem(changedItem);
                                    await interactableCanvasControlItem.InitializeDisplayName();
                                }
                            });
                            break;
                        }
                }
            }
        }

        #endregion

        #region Private Helpers

        private async Task StartFilesystemChangeWatcher(StorageFolder infiniteCanvasFolder)
        {
            if (!_isFilesystemWatcherReady)
            {
                _isFilesystemWatcherReady = true;

                this._filesystemChangeWatcher = await FilesystemChangeWatcher.CreateNew(infiniteCanvasFolder);
                this._filesystemChangeWatcher.OnChangeRegisteredEvent += FilesystemChangeWatcher_OnChangeRegisteredEvent;
            }
        }

        private async Task<SafeWrapperResult> InitializeInfiniteCanvasFolder()
        {
            if (InfiniteCanvasItem != null)
            {
                SafeWrapperResult result = await InfiniteCanvasItem.InitializeCanvasFolder();

                if (!result)
                {
                    return result;
                }
            }
            else
            {
                string folderName = DateTime.Now.ToString(Constants.FileSystem.CANVAS_FILE_FILENAME_DATE_FORMAT);
                folderName = $"{folderName}{Constants.FileSystem.INFINITE_CANVAS_EXTENSION}";

                SafeWrapper<CanvasItem> canvasFolderResult = await associatedCollection.CreateNewCanvasFolder(folderName);

                if (!canvasFolderResult)
                {
                    return canvasFolderResult;
                }
                canvasItem = new InfiniteCanvasItem(canvasFolderResult.Result.AssociatedItem, await canvasFolderResult.Result.SourceItem);

                // Initialize Infinite Canvas
                SafeWrapperResult canvasInitializationResult = await InfiniteCanvasItem.InitializeCanvasFolder();

                if (!canvasInitializationResult)
                {
                    return canvasInitializationResult;
                }
            }

            // Initialize infinite canvas file receiver
            _infiniteCanvasFileReceiver = new InfiniteCanvasFileReceiver(canvasItem);

            return SafeWrapperResult.SUCCESS;
        }

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            base.Dispose();

            if (_isFilesystemWatcherReady && _filesystemChangeWatcher != null)
            {
                _filesystemChangeWatcher.OnChangeRegisteredEvent -= FilesystemChangeWatcher_OnChangeRegisteredEvent;
                _filesystemChangeWatcher.Dispose();
            }

            this.InteractableCanvasControlModel?.Dispose();
            this.ControlView = null;
        }

        #endregion
    }
}
