using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

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
using ClipboardCanvas.Enums;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class InfiniteCanvasViewModel : BaseCanvasViewModel
    {
        private IPasteModel _canvasPasteModel;

        private ICanvasFileReceiverModel _infiniteCanvasFileReceiver;

        private volatile InteractableCanvasControlItemViewModel _currentCanvasItem;

        protected override IPasteModel CanvasPasteModel => null;

        private IInteractableCanvasControlModel InteractableCanvasControlModel => ControlView?.InteractableCanvasModel;

        public IInfiniteCanvasControlView ControlView { get; internal set; }

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
            if (canvasItem == null)
            {
                SafeWrapper<CanvasItem> canvasFolderResult = await associatedCollection.CreateNewCanvasFolder(null);

                if (!AssertNoError(canvasFolderResult))
                {
                    return canvasFolderResult;
                }
                canvasItem = canvasFolderResult;

                // Initialize Infinite Canvas
                SafeWrapperResult canvasInitializationResult = await CanvasHelpers.InitializeInfiniteCanvas(canvasItem);
                if (!AssertNoError(canvasInitializationResult))
                {
                    return canvasInitializationResult;
                }

                // Initialize infinite canvas file receiver
                _infiniteCanvasFileReceiver = new InfiniteCanvasFileReceiver(canvasItem);
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
            await Task.Delay(50);

            // Fetch data to view
            SafeWrapperResult result = await TryFetchDataToView();

            RaiseOnContentLoadedEvent(this, new ContentLoadedEventArgs(contentType, false, false, true));

            return result;
        }

        public override Task<SafeWrapperResult> TrySaveData()
        {
            // TODO: Serialize all items in canvas to JSON
            return Task.FromResult(SafeWrapperResult.CANCEL);
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
            throw new NotImplementedException();
        }

        #endregion
    }
}
