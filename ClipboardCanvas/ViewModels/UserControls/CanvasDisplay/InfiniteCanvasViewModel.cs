﻿using System;
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
using ClipboardCanvas.ViewModels.UserControls.Collections;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class InfiniteCanvasViewModel : BaseCanvasViewModel
    {
        private IPasteModel _canvasPasteModel;

        private ICanvasFileReceiverModel _infiniteCanvasFileReceiver;

        private InfiniteCanvasItem InfiniteCanvasFolder => canvasItem as InfiniteCanvasItem;

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

            // First, set Infinite Canvas folder
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
            await Task.Delay(Constants.UI.CONTROL_LOAD_DELAY);

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
            this.canvasItem = new InfiniteCanvasItem(canvasItem.AssociatedItem, await canvasItem.SourceItem);
            this.contentType = contentType;

            if (IsDisposed)
            {
                return null;
            }

            RaiseOnContentStartedLoadingEvent(this, new ContentStartedLoadingEventArgs(contentType));

            // First, initialize Infinite Canvas folder
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
                var interactableCanvasItem = await InteractableCanvasControlModel?.AddItem(associatedCollection, itemContentType, itemCanvasItem, cancellationToken);
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

            // Always regenerate canvas preview to update it
            await InteractableCanvasControlModel.RegenerateCanvasPreview();

            RaiseOnContentLoadedEvent(this, new ContentLoadedEventArgs(contentType, IsContentLoaded, isContentAsReference, true));

            return SafeWrapperResult.SUCCESS;
        }

        public override async Task<SafeWrapperResult> TrySaveData()
        {
            InfiniteCanvasConfigurationModel canvasConfigurationModel = InteractableCanvasControlModel.ConstructConfigurationModel();

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

        #endregion

        #region Event Handlers

        private async void InteractableCanvasModel_OnInfiniteCanvasSaveRequestedEvent(object sender, InfiniteCanvasSaveRequestedEventArgs e)
        {
            // Save configuration
            SafeWrapperResult saveDataResult = await TrySaveData();
            AssertNoError(saveDataResult); // Only for notification

            // Save canvas image preview
            SafeWrapperResult imagePreviewSaveResult = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                using (IRandomAccessStream fileStream = await InfiniteCanvasFolder.CanvasPreviewImageFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await e.canvasImageStream.AsStreamForRead().CopyToAsync(fileStream.AsStreamForWrite());
                }
            });

            e.canvasImageStream?.Dispose();
        }

        #endregion

        #region Private Helpers

        private async Task<SafeWrapperResult> InitializeInfiniteCanvasFolder()
        {
            if (InfiniteCanvasFolder != null)
            {
                return await InfiniteCanvasFolder.InitializeCanvasFolder();
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
                SafeWrapperResult canvasInitializationResult = await InfiniteCanvasFolder.InitializeCanvasFolder();

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