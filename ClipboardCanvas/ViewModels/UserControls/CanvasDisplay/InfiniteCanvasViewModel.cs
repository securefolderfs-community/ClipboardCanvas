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

            // Get content type from data package
            BaseContentTypeModel contentType = await BaseContentTypeModel.GetContentTypeFromDataPackage(dataPackage);

            // Get correct IPasteModel from contentType
            IPasteModel canvasPasteModel = CanvasHelpers.GetPasteModelFromContentType(contentType, _infiniteCanvasFileReceiver, null);

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
            var interactableCanvasControlCanvasItem = await InteractableCanvasControlModel.AddItem(associatedCollection, contentType, pastedFile, _infiniteCanvasFileReceiver, cancellationToken);

            // Wait for control to load
            await Task.Delay(Constants.UI.CONTROL_LOAD_DELAY);

            // Save data after pasting
            SafeWrapperResult saveDataResult = await SaveConfigurationModel();
            AssertNoError(saveDataResult); // Only for notification

            // Fetch data to view
            SafeWrapperResult fetchDataToViewResult = await interactableCanvasControlCanvasItem.LoadContent();

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

            // Get all items in Infinite Canvas folder
            IEnumerable<IStorageItem> items = await Task.Run(async () => await ((await canvasItem.SourceItem) as StorageFolder).GetItemsAsync());
            List<Task> loadContentTasks = new List<Task>();

            List<InteractableCanvasControlItemViewModel> interactableCanvasList = new List<InteractableCanvasControlItemViewModel>();

            foreach (var item in items)
            {
                if (FilesystemHelpers.IsPathEqualExtension(item.Path, Constants.FileSystem.INFINITE_CANVAS_CONFIGURATION_FILE_EXTENSION)
                    || Path.GetFileName(item.Path) == Constants.FileSystem.INFINITE_CANVAS_PREVIEW_IMAGE_FILENAME)
                {
                    continue;
                }

                // Initialize parameters
                BaseContentTypeModel itemContentType = await BaseContentTypeModel.GetContentType(item, null);
                CanvasItem itemCanvasItem = new CanvasItem(item);

                if (IsDisposed || InteractableCanvasControlModel == null)
                {
                    return null;
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

            // Set configuration model
            var canvasConfigurationModel = JsonConvert.DeserializeObject<InfiniteCanvasConfigurationModel>(readFile);
            InteractableCanvasControlModel.SetConfigurationModel(canvasConfigurationModel);

            // Load item previews
            foreach (var item in interactableCanvasList)
            {
                loadContentTasks.Add(item.LoadContent(true));
            }
            await Task.WhenAll(loadContentTasks);

            // Always regenerate canvas preview on load to update it
            await InteractableCanvasControlModel.RegenerateCanvasPreview();

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

        #endregion

        #region Private Helpers

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

            this.InteractableCanvasControlModel?.Dispose();
            this.ControlView = null;
        }

        #endregion
    }
}
