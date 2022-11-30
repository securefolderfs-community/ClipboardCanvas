using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.IO;
using Newtonsoft.Json;
using CommunityToolkit.Mvvm.Input;
using ClipboardCanvas.GlobalizationExtensions;
using System.Linq;
using System.Collections.Generic;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using CommunityToolkit.WinUI;

using ClipboardCanvas.CanavsPasteModels;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.DataModels.ContentDataModels;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.EventArguments.InfiniteCanvasEventArgs;
using ClipboardCanvas.ViewModels.ContextMenu;
using ClipboardCanvas.ViewModels.UserControls.CanvasPreview;
using ClipboardCanvas.Contexts.Operations;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.Models.Autopaste;
using ClipboardCanvas.Services;
using ClipboardCanvas.UnsafeNative;
using CommunityToolkit.Mvvm.DependencyInjection;
using Vanara.PInvoke;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class InfiniteCanvasViewModel : BaseCanvasViewModel
    {
        #region Private Members

        private CanvasItem? _lastPastedItem;

        private FilesystemChangeWatcher2 _filesystemChangeWatcher;

        private bool _isFilesystemWatcherReady;

        private ICanvasItemReceiverModel _infiniteCanvasFileReceiver;

        #endregion

        #region Properties

        private InfiniteCanvasItem InfiniteCanvasItem => canvasItem as InfiniteCanvasItem;

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

        public IAutopasteService AutopasteService { get; } = Ioc.Default.GetRequiredService<IAutopasteService>();

        #endregion

        #region Constructor

        public InfiniteCanvasViewModel(IBaseCanvasPreviewControlView view, BaseContentTypeModel contentType)
            : base (StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, contentType, view)
        {
        }

        #endregion

        #region Override

        public override async Task<SafeWrapperResult> TryPasteData(DataPackageView dataPackage, CancellationToken cancellationToken)
        {
            _lastPastedItem = null;
            this.cancellationToken = cancellationToken;
            SafeWrapperResult fetchDataToViewResult = SafeWrapperResult.SUCCESS;

            RaiseOnPasteInitiatedEvent(this, new PasteInitiatedEventArgs(false, null, ContentType, AssociatedCollection));

            // First, set Infinite Canvas folder
            SafeWrapperResult initializeInfiniteCanvasFolderResult = await InitializeInfiniteCanvasFolder();
            if (!AssertNoError(initializeInfiniteCanvasFolderResult)) // Default AssertNoError
            {
                return initializeInfiniteCanvasFolderResult;
            }

            if (cancellationToken.IsCancellationRequested) // Check if it's canceled
            {
                DiscardData();
                return SafeWrapperResult.CANCEL;
            }

            // Get content type from data package
            BaseContentTypeModel pastedItemContentType = await BaseContentTypeModel.GetContentTypeFromDataPackage(dataPackage);

            if (pastedItemContentType is InvalidContentTypeDataModel invalidContentType)
            {
                if (invalidContentType.error == (OperationErrorCode.InvalidOperation | OperationErrorCode.NotAFile))
                {
                    // This error code means user tried pasting a folder with Reference Files setting *disabled*
                    AssertNoErrorInfiniteCanvas(invalidContentType.error); // Only for notification
                }
                else
                {
                    return invalidContentType.error;
                }
            }
            else
            {
                // Get correct IPasteModel from contentType
                IPasteModel canvasPasteModel = CanvasHelpers.GetPasteModelFromContentType(pastedItemContentType, _infiniteCanvasFileReceiver, new StatusCenterOperationReceiver());

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
                SafeWrapper<CanvasItem> pastedItem = await canvasPasteModel.PasteData(dataPackage, UserSettings.AlwaysPasteFilesAsReference, cancellationToken);
                _lastPastedItem = pastedItem.Result;

                // We don't need IPasteModel anymore, so dispose it
                canvasPasteModel.Dispose();

                if (!pastedItem)
                {
                    return pastedItem;
                }

                // Add new object to Infinite Canvas
                var interactableCanvasControlItem = await InteractableCanvasControlModel.AddItem(AssociatedCollection, pastedItemContentType, pastedItem, _infiniteCanvasFileReceiver, cancellationToken);

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

                // Notify paste succeeded
                await OnPasteSucceeded(pastedItem);

                if (cancellationToken.IsCancellationRequested) // Check if it's canceled
                {
                    DiscardData();
                    return SafeWrapperResult.CANCEL;
                }

                AssertNoErrorInfiniteCanvas(saveDataResult); // Only for notification

                // Fetch data to view
                fetchDataToViewResult = await interactableCanvasControlItem.LoadContent();
            }

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

            RefreshContextMenuItems();

            RaiseOnContentLoadedEvent(this, new ContentLoadedEventArgs(pastedItemContentType, false, false, CanPasteReference));

            return fetchDataToViewResult;
        }

        public override async Task<SafeWrapperResult> TryLoadExistingData(CanvasItem canvasItem, BaseContentTypeModel contentType, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            this.canvasItem = new InfiniteCanvasItem(canvasItem.AssociatedItem, await canvasItem.SourceItem);
            this.ContentType = contentType;

            if (IsDisposed)
            {
                return null;
            }

            RaiseOnContentStartedLoadingEvent(this, new ContentStartedLoadingEventArgs(contentType));

            // First, initialize Infinite Canvas folder
            SafeWrapperResult initializeInfiniteCanvasFolderResult = await InitializeInfiniteCanvasFolder();
            if (!AssertNoError(initializeInfiniteCanvasFolderResult)) // Default AssertNoError
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
                if (FileHelpers.IsPathEqualExtension(item.Path, Constants.FileSystem.INFINITE_CANVAS_CONFIGURATION_FILE_EXTENSION)
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
                var interactableCanvasItem = await InteractableCanvasControlModel?.AddItem(AssociatedCollection, itemContentType, itemCanvasItem, _infiniteCanvasFileReceiver, cancellationToken);
                interactableCanvasList.Add(interactableCanvasItem);
            }

            // Read Infinite Canvas configuration file
            SafeWrapper<string> readFile = await FilesystemOperations.ReadFileText(InfiniteCanvasItem.ConfigurationFile);
            if (!AssertNoErrorInfiniteCanvas(readFile))
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

            RefreshContextMenuItems();

            RaiseOnContentLoadedEvent(this, new ContentLoadedEventArgs(contentType, IsContentLoaded, false, CanPasteReference));

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
            List<SuggestedActionsControlItemViewModel> actions = new List<SuggestedActionsControlItemViewModel>();

            if (AssociatedItem == null)
            {
                return actions;
            }

            // Paste from clipboard
            var action_paste = new SuggestedActionsControlItemViewModel(
                new AsyncRelayCommand(async () =>
                {
                    SafeWrapper<DataPackageView> dataPackage = ClipboardHelpers.GetClipboardData();

                    await TryPasteData(dataPackage, CanvasPreviewControlViewModel.CanvasPasteCancellationTokenSource.Token);
                }), "PasteFromClipboard".GetLocalized2(), "\uE77F");

            // Open Infinite Canvas folder
            var action_openInfiniteCanvasFolder = new SuggestedActionsControlItemViewModel(
                new AsyncRelayCommand(async () =>
                {
                    await AssociatedCollection.CurrentCollectionItemViewModel.OpenFile();
                }), "OpenInfiniteCanvasFolder".GetLocalized2(), "\uE838");

            actions.Add(action_paste);
            actions.Add(action_openInfiniteCanvasFolder);

            return actions;
        }

        protected override IPasteModel SetCanvasPasteModel()
        {
            return null;
        }

        protected override void RefreshContextMenuItems()
        {
            ContextMenuItems.Clear();

            // Set as Autopaste target
            ContextMenuItems.Add(new MenuFlyoutItemViewModel()
            {
                Command = new AsyncRelayCommand(SetAsAutopasteTarget),
                IconGlyph = "\uE77F",
                Text = "SetAsAutopasteTarget".GetLocalized2()
            });

            // Reset position
            ContextMenuItems.Add(new MenuFlyoutItemViewModel()
            {
                Command = new AsyncRelayCommand(InteractableCanvasControlModel.ResetAllItemPositions),
                IconGlyph = "\uE72C",
                Text = "ResetItemPositions".GetLocalized2()
            });

            // Delete Infinite Canvas
            ContextMenuItems.Add(new MenuFlyoutItemViewModel()
            {
                Command = new AsyncRelayCommand(() => TryDeleteData()),
                IconGlyph = "\uE74D",
                Text = isContentAsReference ? "DeleteReference".GetLocalized2() : "DeleteInfiniteCanvas".GetLocalized2()
            });
        }

        #endregion

        #region Event Handlers

        private async void InteractableCanvasModel_OnInfiniteCanvasSaveRequestedEvent(object sender, InfiniteCanvasSaveRequestedEventArgs e)
        {
            // Save configuration
            SafeWrapperResult saveDataResult = await SaveConfigurationModel();
            AssertNoErrorInfiniteCanvas(saveDataResult); // Only for notification

            // Save canvas image preview
            SafeWrapperResult imagePreviewSaveResult = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                using (IRandomAccessStream fileStream = await InfiniteCanvasItem.CanvasPreviewImageFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    byte[] pixelArray = e.canvasImageBuffer.ToArray();

                    IntPtr hwnd = ApplicationService.GetHwnd(MainWindow.Instance);
                    HMONITOR hmonitor = User32.MonitorFromWindow(hwnd, User32.MonitorFlags.MONITOR_DEFAULTTONULL);
                    SHCore.GetDpiForMonitor(hmonitor, SHCore.MONITOR_DPI_TYPE.MDT_RAW_DPI, out uint rawDpiX, out uint rawDpiY);

                    BitmapEncoder bitmapEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);
                    bitmapEncoder.SetPixelData(BitmapPixelFormat.Bgra8, // RGB with alpha
                                         BitmapAlphaMode.Premultiplied,
                                         (uint)e.pixelWidth,
                                         (uint)e.pixelHeight,
                                         rawDpiX,
                                         rawDpiY,
                                         pixelArray);

                    await bitmapEncoder.FlushAsync();
                }
            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility",
            Justification = "<Pending>")]
        private async void FilesystemChangeWatcher_OnChangeRegisteredEvent(object sender, ChangeRegisteredEventArgs2 e)
        {
            try
            {
                await MainWindow.Instance.DispatcherQueue.EnqueueAsync(async () =>
                {
                    if (e.FullPath.EndsWith(".TMP") || e.FullPath.EndsWith("~tmp"))
                        return;

                    // Reflect changes
                    if (FileHelpers.IsPathEqualExtension(e.FullPath,
                            Constants.FileSystem.INFINITE_CANVAS_CONFIGURATION_FILE_EXTENSION)
                        || Path.GetFileName(e.FullPath) ==
                        Constants.FileSystem.INFINITE_CANVAS_PREVIEW_IMAGE_FILENAME)
                    {
                        return;
                    }

                    string itemParentFolder = Path.GetDirectoryName(e.FullPath);
                    string watchedParentFolder = (await InfiniteCanvasItem.SourceItem).Path;
                    if (itemParentFolder != watchedParentFolder)
                    {
                        return;
                    }

                    IStorageItem changedItem = await StorageHelpers.ToStorageItem<IStorageItem>(e.FullPath);

                    switch (e.ChangeType)
                    {
                        case WatcherChangeTypes.Created:
                        {
                            if (changedItem == null ||
                                InteractableCanvasControlModel.ContainsItem(
                                    InteractableCanvasControlModel.FindItem(changedItem.Path)))
                            {
                                return;
                            }

                            BaseContentTypeModel contentType =
                                await BaseContentTypeModel.GetContentType(changedItem, null);
                            if (contentType != null)
                            {
                                CanvasItem canvasItem = new CanvasItem(changedItem);

                                var interactableCanvasControlItem =
                                    await InteractableCanvasControlModel.AddItem(AssociatedCollection, contentType,
                                        canvasItem, _infiniteCanvasFileReceiver, cancellationToken);
                                if (interactableCanvasControlItem != null)
                                {
                                    await interactableCanvasControlItem.LoadContent(true);
                                }
                            }

                            break;
                        }

                        case WatcherChangeTypes.Deleted:
                        {
                            InteractableCanvasControlModel.RemoveItem(InteractableCanvasControlModel.FindItem(e.FullPath));
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
                                    var interactableCanvasControlItem =
                                        InteractableCanvasControlModel.FindItem(e.OldPath);

                                    if (interactableCanvasControlItem != null)
                                    {
                                        interactableCanvasControlItem.CanvasItem.DangerousUpdateItem(changedItem);
                                        await interactableCanvasControlItem.InitializeDisplayName();

                                        // Since it was renamed, configuration model needs to be updated too!
                                        await SaveConfigurationModel();
                                    }
                                }
                            }

                            break;
                        }

                        case WatcherChangeTypes.Changed:
                        {
                            var interactableCanvasControlItem = InteractableCanvasControlModel.FindItem(e.FullPath);
                            if (interactableCanvasControlItem != null)
                            {
                                await interactableCanvasControlItem.LoadContent();
                                await InteractableCanvasControlModel.RegenerateCanvasPreview();
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

        #endregion

        #region Private Helpers

        private async Task SetAsAutopasteTarget()
        {
            var autopasteTargetBridge = new AutopasteTargetWrapper("OOBEInfiniteCanvasTitle".GetLocalized2() + " - " + AssociatedCollection.DisplayName, (await SourceItem).Path,
                async (dataPackage, cancellationToken) =>
                {
                    var result = await TryPasteData(dataPackage, cancellationToken);
                    return new SafeWrapper<CanvasItem>(_lastPastedItem, result.Details);
                });
            AutopasteService.UpdateAutopasteTarget(autopasteTargetBridge);
        }

        private bool AssertNoErrorInfiniteCanvas(SafeWrapperResult result)
        {
            return AssertNoError(result, TimeSpan.FromMilliseconds(Constants.UI.CanvasContent.INFINITE_CANVAS_ERROR_SHOW_TIME));
        }

        private async Task StartFilesystemChangeWatcher(StorageFolder infiniteCanvasFolder)
        {
            if (!_isFilesystemWatcherReady)
            {
                _isFilesystemWatcherReady = true;

                this._filesystemChangeWatcher = new FilesystemChangeWatcher2(infiniteCanvasFolder.Path);
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

                result = await SetContentMode();

                if (!result)
                {
                    return result;
                }
            }
            else
            {
                string folderName = DateTime.Now.ToString(Constants.FileSystem.CANVAS_FILE_FILENAME_DATE_FORMAT);
                folderName = $"{folderName}{Constants.FileSystem.INFINITE_CANVAS_EXTENSION}";

                SafeWrapper<CanvasItem> canvasFolderResult = await AssociatedCollection.CreateNewCanvasFolder(folderName);

                if (!canvasFolderResult)
                {
                    return canvasFolderResult;
                }
                canvasItem = new InfiniteCanvasItem(canvasFolderResult.Result.AssociatedItem, await canvasFolderResult.Result.SourceItem);
                collectionItemViewModel = AssociatedCollection.FindCollectionItem(canvasItem);

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
        }

        #endregion
    }
}
