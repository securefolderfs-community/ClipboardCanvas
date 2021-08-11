using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.IO;
using Newtonsoft.Json;

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
using System.Collections.Generic;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class InfiniteCanvasViewModel : BaseCanvasViewModel
    {
        private IPasteModel _canvasPasteModel;

        private ICanvasFileReceiverModel _infiniteCanvasFileReceiver;

        private volatile InteractableCanvasControlItemViewModel _currentCanvasItem;

        protected override IPasteModel CanvasPasteModel => null;

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

        public InfiniteCanvasViewModel(IBaseCanvasPreviewControlView view)
            : base (StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, new InfiniteCanvasContentType(), view)
        {
        }

        #endregion

        #region Override

        public override async Task<SafeWrapperResult> TryPasteData(DataPackageView dataPackage, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;

            RaiseOnPasteInitiatedEvent(this, new PasteInitiatedEventArgs(false, null));

            // First, set Infinite Canvas folder if null
            SafeWrapperResult result = await InitializeInfiniteCanvasFolder();
            if (!AssertNoError(result))
            {
                return result;
            }

            // Get content type from data package
            BaseContentTypeModel contentType = await BaseContentTypeModel.GetContentTypeFromDataPackage(dataPackage);

            // Get correct IPasteModel from contentType
            _canvasPasteModel = CanvasHelpers.GetPasteModelFromContentType(contentType, _infiniteCanvasFileReceiver, null);

            if (_canvasPasteModel == null)
            {
                return BaseContentTypeModel.CannotDisplayContentForTypeResult;
            }

            // Paste data
            SafeWrapper<CanvasItem> pastedFile = await _canvasPasteModel.PasteData(dataPackage, UserSettings.AlwaysPasteFilesAsReference, cancellationToken);

            if (!pastedFile)
            {
                return pastedFile;
            }
            
            // Add new object to Infinite Canvas
            _currentCanvasItem = await InteractableCanvasControlModel.AddItem(associatedCollection, contentType, pastedFile, cancellationToken);

            // Wait for control to load
            await Task.Delay(10);

            // Save data after pasting
            SafeWrapperResult saveDataResult = await TrySaveData();
            AssertNoError(saveDataResult); // Only for notification

            // Fetch data to view
            result = await TryFetchDataToView();

            RaiseOnContentLoadedEvent(this, new ContentLoadedEventArgs(contentType, false, false, true));

            return result;
        }

        public override async Task<SafeWrapperResult> TryLoadExistingData(CanvasItem canvasItem, BaseContentTypeModel contentType, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            this.canvasItem = canvasItem;
            this.contentType = contentType;

            RaiseOnContentStartedLoadingEvent(this, new ContentStartedLoadingEventArgs(contentType));

            // First, set Infinite Canvas folder if null
            SafeWrapperResult result = await InitializeInfiniteCanvasFolder();
            if (!AssertNoError(result))
            {
                return result;
            }

            // Get all items in Infinite Canvas folder
            IEnumerable<IStorageItem> items = await Task.Run(async () => await ((await canvasItem.SourceItem) as StorageFolder).GetItemsAsync());
            List<Task> loadContentTasks = new List<Task>();

            List<InteractableCanvasControlItemViewModel> interactableCanvasList = new List<InteractableCanvasControlItemViewModel>();

            foreach (var item in items)
            {
                if (item.Path.EndsWith(Constants.FileSystem.INFINITE_CANVAS_CONFIGURATION_FILE_EXTENSION))
                {
                    continue;
                }

                // Initialize parameters
                BaseContentTypeModel itemContentType = await BaseContentTypeModel.GetContentType(item, null);
                CanvasItem itemCanvasItem = new CanvasItem(item);

                // Add to canvas
                var interactableCanvasItem = await InteractableCanvasControlModel.AddItem(associatedCollection, itemContentType, itemCanvasItem, cancellationToken);
                interactableCanvasList.Add(interactableCanvasItem);
            }

            // Get Infinite Canvas configuration model
            string configurationFileName = Constants.FileSystem.INFINITE_CANVAS_CONFIGURATION_FILENAME;
            string configurationPath = Path.Combine((await sourceFolder).Path, configurationFileName);
            StorageFile configurationFile = await StorageHelpers.ToStorageItem<StorageFile>(configurationPath);

            SafeWrapper<string> readFile = await FilesystemOperations.ReadFileText(configurationFile);
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

            RaiseOnContentLoadedEvent(this, new ContentLoadedEventArgs(contentType, IsContentLoaded, isContentAsReference, true));

            return SafeWrapperResult.SUCCESS;
        }

        public override async Task<SafeWrapperResult> TrySaveData()
        {
            InfiniteCanvasConfigurationModel canvasConfigurationModel = InteractableCanvasControlModel.GetConfigurationModel();

            string configurationFileName = Constants.FileSystem.INFINITE_CANVAS_CONFIGURATION_FILENAME;
            string configurationPath = Path.Combine((await sourceFolder).Path, configurationFileName);

            SafeWrapper<StorageFile> configurationFile = await StorageHelpers.ToStorageItemWithError<StorageFile>(configurationPath);
            if (!configurationFile)
            {
                return configurationFile;
            }

            string serializedConfig = JsonConvert.SerializeObject(canvasConfigurationModel, Formatting.Indented);
            SafeWrapperResult writeConfigResult = await FilesystemOperations.WriteFileText(configurationFile.Result, serializedConfig);

            return writeConfigResult;
        }

        protected override Task<SafeWrapperResult> SetData(DataPackageView dataPackage)
        {
            return Task.FromResult(SafeWrapperResult.CANCEL);
        }

        protected override Task<SafeWrapperResult> SetDataFromExistingFile(IStorageItem item)
        {
            return Task.FromResult(SafeWrapperResult.CANCEL);
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            return await _currentCanvasItem.LoadContent();
        }

        protected override Task<SafeWrapper<CollectionItemViewModel>> TrySetFileWithExtension()
        {
            return Task.FromResult<SafeWrapper<CollectionItemViewModel>>((null, SafeWrapperResult.CANCEL));
        }

        #endregion

        #region Event Handlers

        private async void InteractableCanvasModel_OnInfiniteCanvasSaveRequestedEvent(object sender, EventArguments.InfiniteCanvasEventArgs.InfiniteCanvasSaveRequestedEventArgs e)
        {
            SafeWrapperResult saveDataResult = await TrySaveData();
            AssertNoError(saveDataResult); // Only for notification
        }

        #endregion

        #region Private Helpers

        private async Task<SafeWrapperResult> InitializeInfiniteCanvasFolder()
        {
            if (canvasItem == null)
            {
                SafeWrapper<CanvasItem> canvasFolderResult = await associatedCollection.CreateNewCanvasFolder(null);

                if (!canvasFolderResult)
                {
                    return canvasFolderResult;
                }
                canvasItem = canvasFolderResult;

                // Initialize Infinite Canvas
                SafeWrapperResult canvasInitializationResult = await CanvasHelpers.InitializeInfiniteCanvas(canvasItem);
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
