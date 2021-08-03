using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Enums;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasPreview
{
    public abstract class BaseCanvasPreviewControlViewModel : BaseReadOnlyCanvasPreviewControlViewModel<BaseCanvasViewModel>, ICanvasPreviewModel, IDisposable
    {
        #region Public Properties

        public CanvasType RequestedCanvasType { get; set; }

        #endregion

        #region Events

        public event EventHandler<OpenNewCanvasRequestedEventArgs> OnOpenNewCanvasRequestedEvent;

        public event EventHandler<PasteInitiatedEventArgs> OnPasteInitiatedEvent;

        public event EventHandler<FileCreatedEventArgs> OnFileCreatedEvent;

        public event EventHandler<FileModifiedEventArgs> OnFileModifiedEvent;

        #endregion

        #region Constructor

        public BaseCanvasPreviewControlViewModel(IBaseCanvasPreviewControlView view)
            : base(view)
        {
        }

        #endregion

        #region ICanvasPreviewModel

        public async Task<SafeWrapperResult> TryPasteData(DataPackageView dataPackage, CancellationToken cancellationToken)
        {
            BasePastedContentTypeDataModel contentType;
            
            if (RequestedCanvasType == CanvasType.OneCanvas)
            {
                contentType = await BasePastedContentTypeDataModel.GetContentTypeFromDataPackage(dataPackage);
            }
            else
            {
                contentType = new InfiniteCanvasContentType();
            }

            SafeWrapperResult result = await InitializeViewModelAndPaste(dataPackage, contentType, cancellationToken);

            if (!result)
            {
                RaiseOnErrorOccurredEvent(this, new ErrorOccurredEventArgs(result, result.Message));
            }

            return result;
        }

        public async Task<SafeWrapperResult> TrySaveData()
        {
            if (CanvasViewModel == null)
            {
                return CanvasNullResult;
            }

            return await CanvasViewModel.TrySaveData();
        }

        public async Task<SafeWrapperResult> PasteOverrideReference()
        {
            if (CanvasViewModel == null)
            {
                return CanvasNullResult;
            }

            return await CanvasViewModel.PasteOverrideReference();
        }

        public void OpenNewCanvas()
        {
            CanvasViewModel?.OpenNewCanvas();
        }

        public async Task<IEnumerable<SuggestedActionsControlItemViewModel>> GetSuggestedActions()
        {
            if (CanvasViewModel == null)
            {
                return null;
            }

            return await CanvasViewModel.GetSuggestedActions();
        }

        #endregion

        #region Protected Helpers

        /// <summary>
        /// Decide and initialize View Model from data package
        /// </summary>
        /// <param name="dataPackage"></param>
        /// <returns></returns>
        protected virtual async Task<SafeWrapperResult> InitializeViewModelAndPaste(DataPackageView dataPackage, BasePastedContentTypeDataModel contentType, CancellationToken cancellationToken)
        {
            // Decide content type and initialize view model

            // Discard any left over data if we're already pasting to canvas that is filled
            DiscardData();

            if (contentType is InvalidContentTypeDataModel invalidContentType)
            {
                return invalidContentType.error;
            }

            if (InitializeViewModelFromContentType(contentType))
            {
                RaiseOnProgressReportedEvent(this, new ProgressReportedEventArgs(0.0f, true, CanvasPageProgressType.MainCanvasProgressBar));
                SafeWrapperResult result = await CanvasViewModel.TryPasteData(dataPackage, cancellationToken);
                RaiseOnProgressReportedEvent(this, new ProgressReportedEventArgs(100.0f, true, CanvasPageProgressType.MainCanvasProgressBar));

                return result;
            }
            else
            {
                return new SafeWrapperResult(OperationErrorCode.AccessUnauthorized, null, "Cannot paste data.");
            }
        }

        #endregion

        #region Event Handlers

        private void CanvasViewModel_OnOpenNewCanvasRequestedEvent(object sender, OpenNewCanvasRequestedEventArgs e)
        {
            RaiseOnOpenNewCanvasRequestedEvent(sender, e);
        }

        private void CanvasViewModel_OnPasteInitiatedEvent(object sender, PasteInitiatedEventArgs e)
        {
            RaiseOnPasteInitiatedEvent(sender, e);
        }

        private void CanvasViewModel_OnFileCreatedEvent(object sender, FileCreatedEventArgs e)
        {
            OnFileCreatedEvent(sender, e);
        }

        private void CanvasViewModel_OnFileModifiedEvent(object sender, FileModifiedEventArgs e)
        {
            RaiseOnFileModifiedEvent(sender, e);
        }

        #endregion

        #region Event Raisers

        protected void RaiseOnOpenNewCanvasRequestedEvent(object s, OpenNewCanvasRequestedEventArgs e) => OnOpenNewCanvasRequestedEvent?.Invoke(s, e);

        protected void RaiseOnPasteInitiatedEvent(object s, PasteInitiatedEventArgs e) => OnPasteInitiatedEvent?.Invoke(s, e);

        protected void RaiseOnFileCreatedEvent(object s, FileCreatedEventArgs e) => OnFileCreatedEvent?.Invoke(s, e);

        protected void RaiseOnFileModifiedEvent(object s, FileModifiedEventArgs e) => OnFileModifiedEvent?.Invoke(s, e);

        #endregion

        #region Override

        protected override void HookCanvasViewModelEvents()
        {
            base.HookCanvasViewModelEvents();

            if (CanvasViewModel != null)
            {
                CanvasViewModel.OnOpenNewCanvasRequestedEvent += CanvasViewModel_OnOpenNewCanvasRequestedEvent;
                CanvasViewModel.OnPasteInitiatedEvent += CanvasViewModel_OnPasteInitiatedEvent;
                CanvasViewModel.OnFileCreatedEvent += CanvasViewModel_OnFileCreatedEvent;
                CanvasViewModel.OnFileModifiedEvent += CanvasViewModel_OnFileModifiedEvent;
            }
        }

        protected override void UnhookCanvasViewModelEvents()
        {
            base.UnhookCanvasViewModelEvents();

            if (CanvasViewModel != null)
            {
                CanvasViewModel.OnOpenNewCanvasRequestedEvent -= CanvasViewModel_OnOpenNewCanvasRequestedEvent;
                CanvasViewModel.OnPasteInitiatedEvent -= CanvasViewModel_OnPasteInitiatedEvent;
                CanvasViewModel.OnFileCreatedEvent -= CanvasViewModel_OnFileCreatedEvent;
                CanvasViewModel.OnFileModifiedEvent -= CanvasViewModel_OnFileModifiedEvent;
            }
        }

        #endregion
    }
}
