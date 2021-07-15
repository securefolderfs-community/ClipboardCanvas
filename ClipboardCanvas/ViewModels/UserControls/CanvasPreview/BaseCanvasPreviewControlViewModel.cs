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
        #region Events

        public event EventHandler<OpenNewCanvasRequestedEventArgs> OnOpenNewCanvasRequestedEvent;

        public event EventHandler<PasteInitiatedEventArgs> OnPasteInitiatedEvent;

        public event EventHandler<FileCreatedEventArgs> OnFileCreatedEvent;

        public event EventHandler<FileModifiedEventArgs> OnFileModifiedEvent;

        public event EventHandler<ProgressReportedEventArgs> OnProgressReportedEvent;

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
            SafeWrapperResult result = await InitializeViewModelAndPaste(dataPackage, cancellationToken);

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
        protected virtual async Task<SafeWrapperResult> InitializeViewModelAndPaste(DataPackageView dataPackage, CancellationToken cancellationToken)
        {
            // Decide content type and initialize view model

            // Discard any left over data if we're already pasting to canvas that is filled
            DiscardData();

            // From raw clipboard data
            if (dataPackage.Contains(StandardDataFormats.Bitmap))
            {
                // Image
                InitializeViewModel(() => new ImageCanvasViewModel(view));
            }
            else if (dataPackage.Contains(StandardDataFormats.Text))
            {
                SafeWrapper<string> text = await SafeWrapperRoutines.SafeWrapAsync(() => dataPackage.GetTextAsync().AsTask());

                if (!text)
                {
                    Debugger.Break(); // What!?
                    return new SafeWrapperResult(OperationErrorCode.AccessUnauthorized, "Couldn't retrieve clipboard data");
                }

                // Check if it's url
                if (StringHelpers.IsUrl(text))
                {
                    // The url may point to file
                    if (StringHelpers.IsUrlFile(text))
                    {
                        // Image
                        InitializeViewModel(() => new ImageCanvasViewModel(view));
                    }
                    else
                    {
                        // Webpage link
                        //InitializeViewModel(() => new WebViewCanvasViewModel(_view, WebViewCanvasMode.ReadWebsite, CanvasPreviewMode.InteractionAndPreview));
                        if (App.AppSettings.UserSettings.PrioritizeMarkdownOverText)
                        {
                            // Markdown
                            InitializeViewModel(() => new MarkdownCanvasViewModel(view));
                        }
                        else
                        {
                            // Normal text
                            InitializeViewModel(() => new TextCanvasViewModel(view));
                        }
                    }
                }
                else
                {
                    if (App.AppSettings.UserSettings.PrioritizeMarkdownOverText)
                    {
                        // Markdown
                        InitializeViewModel(() => new MarkdownCanvasViewModel(view));
                    }
                    else
                    {
                        // Normal text
                        InitializeViewModel(() => new TextCanvasViewModel(view));
                    }
                }
            }
            else if (dataPackage.Contains(StandardDataFormats.StorageItems)) // From clipboard storage items
            {
                IReadOnlyList<IStorageItem> items = await dataPackage.GetStorageItemsAsync();

                if (items.Count > 1)
                {
                    // TODO: More than one item, paste in Boundless Canvas
                }
                else if (items.Count == 1)
                {
                    // One item, decide view model for it
                    IStorageItem item = items.First();

                    BasePastedContentTypeDataModel contentType = await BasePastedContentTypeDataModel.GetContentType(item, null);
                    if (contentType is InvalidContentTypeDataModel)
                    {
                        return new SafeWrapperResult(OperationErrorCode.NotFound, "Couldn't get content type for provided data");
                    }

                    InitializeViewModelFromContentType(contentType);
                }
                else
                {
                    // No items
                    return new SafeWrapperResult(OperationErrorCode.AccessUnauthorized, "Couldn't retrieve clipboard data");
                }
            }

            if (CanvasViewModel == null)
            {
                return new SafeWrapperResult(OperationErrorCode.AccessUnauthorized, "Couldn't retrieve clipboard data");
            }
            else
            {
                RaiseOnProgressReportedEvent(this, new ProgressReportedEventArgs(0.0f, true, CanvasPageProgressType.MainCanvasProgressBar));
                SafeWrapperResult result = await CanvasViewModel.TryPasteData(dataPackage, cancellationToken);
                RaiseOnProgressReportedEvent(this, new ProgressReportedEventArgs(100.0f, true, CanvasPageProgressType.MainCanvasProgressBar));

                return result;
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

        private void CanvasViewModel_OnProgressReportedEvent(object sender, ProgressReportedEventArgs e)
        {
            RaiseOnProgressReportedEvent(sender, e);
        }

        #endregion

        #region Event Raisers

        protected void RaiseOnOpenNewCanvasRequestedEvent(object s, OpenNewCanvasRequestedEventArgs e) => OnOpenNewCanvasRequestedEvent?.Invoke(s, e);

        protected void RaiseOnPasteInitiatedEvent(object s, PasteInitiatedEventArgs e) => OnPasteInitiatedEvent?.Invoke(s, e);

        protected void RaiseOnFileCreatedEvent(object s, FileCreatedEventArgs e) => OnFileCreatedEvent?.Invoke(s, e);

        protected void RaiseOnFileModifiedEvent(object s, FileModifiedEventArgs e) => OnFileModifiedEvent?.Invoke(s, e);

        protected void RaiseOnProgressReportedEvent(object s, ProgressReportedEventArgs e) => OnProgressReportedEvent?.Invoke(s, e);

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
                CanvasViewModel.OnProgressReportedEvent += CanvasViewModel_OnProgressReportedEvent;
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
                CanvasViewModel.OnProgressReportedEvent -= CanvasViewModel_OnProgressReportedEvent;
            }
        }

        #endregion
    }
}
