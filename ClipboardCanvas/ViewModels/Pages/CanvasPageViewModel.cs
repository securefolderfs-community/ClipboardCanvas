using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Threading;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml.Input;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using System.Collections.Generic;
using Windows.Storage;
using System.Collections.ObjectModel;

using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.Enums;
using ClipboardCanvas.ViewModels.ContextMenu;
using ClipboardCanvas.ViewModels.UserControls.CanvasPreview;
using ClipboardCanvas.DataModels.ContentDataModels;

namespace ClipboardCanvas.ViewModels.Pages
{
    public class CanvasPageViewModel : ObservableObject, IPasteCanvasPageModel, IDisposable
    {
        #region Private Members

        private readonly ICanvasPageView _view;

        private const string DEFAULT_TITLE_TEXT = "Press Ctrl + V to paste in content";

        private const string DRAG_TITLE_TEXT = "Release to paste in content";

        private bool _contentFinishedLoading;

        private bool _isInTemporaryErrorLoadPhase;

        #endregion

        #region Public Properties

        public ICollectionModel CollectionModel => _view?.AssociatedCollectionModel;

        public ICanvasPreviewModel PasteCanvasModel => _view?.CanvasPreviewModel;

        private CanvasType _RequestedCanvasType;
        public CanvasType RequestedCanvasType
        {
            get => _RequestedCanvasType;
            set
            {
                if (_RequestedCanvasType != value)
                {
                    _RequestedCanvasType = value;

                    OnPropertyChanged(nameof(SwitchCanvasModeText));
                    OnPropertyChanged(nameof(CanvasTypeText));
                }

                CollectionModel.AssociatedCanvasType = value;
            }
        }

        public ObservableCollection<BaseMenuFlyoutItemViewModel> CanvasContextMenuItems
        {
            get => PasteCanvasModel?.ContextMenuItems;
        }

        private bool _NewCanvasScreenLoad = true;
        public bool NewCanvasScreenLoad
        {
            get => _NewCanvasScreenLoad;
            set => SetProperty(ref _NewCanvasScreenLoad, value);
        }

        public string SwitchCanvasModeText
        {
            get => RequestedCanvasType == CanvasType.OneCanvas ? "Switch to Infinite Canvas" : "Switch to One Canvas";
        }

        private bool _InfiniteCanvasCaptionLoad;
        public bool InfiniteCanvasCaptionLoad
        {
            get => _InfiniteCanvasCaptionLoad;
            set => SetProperty(ref _InfiniteCanvasCaptionLoad, value);
        }

        private bool _CanvasRingLoad;
        public bool CanvasRingLoad
        {
            get => _CanvasRingLoad;
            set => SetProperty(ref _CanvasRingLoad, value);
        }

        private bool _InfiniteCanvasProgressLoad = false;
        public bool InfiniteCanvasProgressLoad
        {
            get => _InfiniteCanvasProgressLoad;
            set => SetProperty(ref _InfiniteCanvasProgressLoad, value);
        }

        private string _TitleText = DEFAULT_TITLE_TEXT;
        public string TitleText
        {
            get => _TitleText;
            set => SetProperty(ref _TitleText, value);
        }

        public string CanvasTypeText
        {
            get => RequestedCanvasType == CanvasType.OneCanvas ? "You're in One Canvas mode" : "You're in Infinite Canvas mode";
        }

        private string _TipText;
        public string TipText
        {
            get => _TipText;
            set => SetProperty(ref _TipText, value);
        }

        private bool _TipTextLoad;
        public bool TipTextLoad
        {
            get => _TipTextLoad;
            set => SetProperty(ref _TipTextLoad, value);
        }

        private bool _ErrorTextLoad = true;
        public bool ErrorTextLoad
        {
            get => _ErrorTextLoad;
            set => SetProperty(ref _ErrorTextLoad, value);
        }

        private string _ErrorText;
        public string ErrorText
        {
            get => _ErrorText;
            set => SetProperty(ref _ErrorText, value);
        }

        private bool _PastedAsReferenceLoad;
        public bool PastedAsReferenceLoad
        {
            get => _PastedAsReferenceLoad;
            set => SetProperty(ref _PastedAsReferenceLoad, value);
        }

        private bool _OverrideReferenceEnabled = true;
        public bool OverrideReferenceEnabled
        {
            get => _OverrideReferenceEnabled;
            set => SetProperty(ref _OverrideReferenceEnabled, value);
        }

        #endregion

        #region Commands

        public ICommand DefaultKeyboardAcceleratorInvokedCommand { get; private set; }

        public ICommand DragEnterCommand { get; private set; }

        public ICommand DragLeaveCommand { get; private set; }

        public ICommand DropCommand { get; private set; }

        public ICommand OverrideReferenceCommand { get; private set; }

        public ICommand CanvasContextMenuOpeningCommand { get; private set; }

        public ICommand SwitchCanvasModeCommand { get; private set; }

        #endregion

        #region Constructor

        public CanvasPageViewModel(ICanvasPageView view)
        {
            this._view = view;

            HookEvents();

            (PasteCanvasModel as BaseCanvasPreviewControlViewModel).GetRequestedCanvasTypeFunc = () => RequestedCanvasType; // TODO: This class casting is bad, refactor entire code

            // Create commands
            DefaultKeyboardAcceleratorInvokedCommand = new AsyncRelayCommand<KeyboardAcceleratorInvokedEventArgs>(DefaultKeyboardAcceleratorInvoked);
            DragEnterCommand = new AsyncRelayCommand<DragEventArgs>(DragEnter);
            DragLeaveCommand = new RelayCommand<DragEventArgs>(DragLeave);
            DropCommand = new AsyncRelayCommand<DragEventArgs>(Drop);
            OverrideReferenceCommand = new AsyncRelayCommand(OverrideReference);
            CanvasContextMenuOpeningCommand = new RelayCommand(CanvasContextMenuOpening);
            SwitchCanvasModeCommand = new RelayCommand(SwitchCanvasMode);
        }

        #endregion

        #region Command Implementation

        private async Task DefaultKeyboardAcceleratorInvoked(KeyboardAcceleratorInvokedEventArgs e)
        {
            e.Handled = true;
            bool ctrl = e.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Control);
            bool shift = e.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Shift);
            bool alt = e.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Menu);
            bool win = e.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Windows);
            VirtualKey vkey = e.KeyboardAccelerator.Key;
            uint uVkey = (uint)e.KeyboardAccelerator.Key;

            switch (c: ctrl, s: shift, a: alt, w: win, k: vkey)
            {
                case (c: true, s: false, a: false, w: false, k: VirtualKey.V):
                    {
                        await PasteDataInternal();
                        break;
                    }

                case (c: true, s: false, a: false, w: false, k: VirtualKey.C):
                    {
                        PasteCanvasModel?.SetDataToClipboard();
                        break;
                    }

                case (c: false, s: _, a: false, w: false, k: VirtualKey.Delete):
                    {
                        if (PasteCanvasModel != null)
                        {
                            await PasteCanvasModel.TryDeleteData();
                        }
                        break;
                    }
            }
        }

        private async Task DragEnter(DragEventArgs e)
        {
            DragOperationDeferral deferral = null;

            try
            {
                deferral = e.GetDeferral();

                // Make sure we don't check dragged Infinite Canvas object
                if (e.DataView.Properties.ContainsKey(Constants.UI.CanvasContent.INFINITE_CANVAS_DRAGGED_OBJECT_ID))
                {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                }
                else if (e.DataView.Contains(StandardDataFormats.Text))
                {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                    TitleText = DRAG_TITLE_TEXT;
                }
                else if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    SafeWrapper<IReadOnlyList<IStorageItem>> draggedItems = await SafeWrapperRoutines.SafeWrapAsync(async () =>
                        await e.DataView.GetStorageItemsAsync());

                    if (draggedItems)
                    {
                        if (CollectionModel.IsOnNewCanvas)
                        {
                            e.AcceptedOperation = DataPackageOperation.Copy;
                            TitleText = DRAG_TITLE_TEXT;
                            return;
                        }
                        else
                        {
                            bool canPaste = true;

                            foreach (var item in draggedItems.Result)
                            {
                                if (item.Path == (await CollectionModel.CurrentCollectionItemViewModel.SourceItem).Path)
                                {
                                    canPaste = false;
                                    break;
                                }
                            }

                            if (canPaste)
                            {
                                e.AcceptedOperation = DataPackageOperation.Copy;
                                TitleText = DRAG_TITLE_TEXT;
                                return;
                            }
                        }
                    }
                }
                else
                {
                    e.AcceptedOperation = DataPackageOperation.None;
                    TitleText = DEFAULT_TITLE_TEXT;
                }
            }
            finally
            {
                e.Handled = true;
                deferral?.Complete();
            }
        }

        private void DragLeave(DragEventArgs e)
        {
            TitleText = DEFAULT_TITLE_TEXT;
        }

        private async Task Drop(DragEventArgs e)
        {
            DragOperationDeferral deferral = null;
            
            try
            {
                deferral = e.GetDeferral();

                // Ignore dragged Infinite Canvas object
                if (e.DataView.Properties.ContainsKey(Constants.UI.CanvasContent.INFINITE_CANVAS_DRAGGED_OBJECT_ID))
                {
                    return;
                }

                DataPackageView dataPackage = e.DataView;
                await PasteDataInternal(dataPackage);
            }
            finally
            {
                e.Handled = true;
                deferral?.Complete();
            }
        }

        private async Task OverrideReference()
        {
            OverrideReferenceEnabled = false;
            SafeWrapperResult result = await PasteCanvasModel.PasteOverrideReference();

            if (result)
            {
                PastedAsReferenceLoad = false;
            }
            else
            {
                // TODO: Show error here
            }

            OverrideReferenceEnabled = true;
        }

        private void CanvasContextMenuOpening()
        {
            // Always refresh all context menu items
            OnPropertyChanged(nameof(CanvasContextMenuItems));
        }

        private void SwitchCanvasMode()
        {
            if (RequestedCanvasType == CanvasType.InfiniteCanvas)
            {
                RequestedCanvasType = CanvasType.OneCanvas;
            }
            else
            {
                RequestedCanvasType = CanvasType.InfiniteCanvas;
            }
        }

        #endregion

        #region Event Handlers

        private async void PasteCanvasModel_OnTipTextUpdateRequestedEvent(object sender, TipTextUpdateRequestedEventArgs e)
        {
            await SetTipText(e.infoText, e.tipShowDelay);
        }

        private void PasteCanvasModel_OnProgressReportedEvent(object sender, ProgressReportedEventArgs e)
        {
            if (e.value == 100.0f)
            {
                ShowOrHideCanvasLoadingProgress(false, e.contentType);
            }
            else
            {
                ShowOrHideCanvasLoadingProgress(true, e.contentType);
            }
        }

        private async void PasteCanvasModel_OnErrorOccurredEvent(object sender, ErrorOccurredEventArgs e)
        {
            ErrorTextLoad = true;
            ErrorText = e.errorMessage;
            ShowOrHideCanvasLoadingProgress(false, null);

            TimeSpan errorMessageDelay = e.errorMessageAutoHide;
            if (e.contentType is not InfiniteCanvasContentType)
            {
                NewCanvasScreenLoad = false;
                TipTextLoad = false;
                PasteCanvasModel?.DiscardData();
            }
            else
            {
                if (errorMessageDelay == TimeSpan.Zero)
                {
                    errorMessageDelay = TimeSpan.FromMilliseconds(Constants.UI.CanvasContent.INFINITE_CANVAS_ERROR_SHOW_TIME);
                }
            }

            if (errorMessageDelay != TimeSpan.Zero)
            {
                _isInTemporaryErrorLoadPhase = true;
                await Task.Delay(errorMessageDelay);
                ErrorText = null;
                ErrorTextLoad = false;
            }

            _isInTemporaryErrorLoadPhase = false;
        }

        private async void PasteCanvasModel_OnContentStartedLoadingEvent(object sender, ContentStartedLoadingEventArgs e)
        {
            PastedAsReferenceLoad = false;
            InfiniteCanvasCaptionLoad = e.contentType is InfiniteCanvasContentType;
            await PrepareForContentStartLoading(e.contentType);
        }

        private void PasteCanvasModel_OnContentLoadedEvent(object sender, ContentLoadedEventArgs e)
        {
            PastedAsReferenceLoad = e.pastedAsReference;
            OverrideReferenceEnabled = e.canPasteReference;
            ShowOrHideCanvasLoadingProgress(false, e.contentType);
            _contentFinishedLoading = true;
            NewCanvasScreenLoad = false;
            TipTextLoad = false;
            ErrorTextLoad = _isInTemporaryErrorLoadPhase;

            _view?.FinishConnectedAnimation();
        }

        private void PasteCanvasModel_OnContentLoadFailedEvent(object sender, ErrorOccurredEventArgs e)
        {
            _contentFinishedLoading = true;
            ShowOrHideCanvasLoadingProgress(false, null);
            PastedAsReferenceLoad = false;

            _view?.FinishConnectedAnimation();
        }

        private async void PasteCanvasModel_OnPasteInitiatedEvent(object sender, PasteInitiatedEventArgs e)
        {
            await PrepareForContentStartLoading(e.contentType);
        }

        #endregion

        #region IPasteCanvasPageModel

        public async Task SetTipText(string text)
        {
            await SetTipText(text, TimeSpan.Zero);
        }

        public async Task SetTipText(string text, TimeSpan tipShowDelay)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (tipShowDelay != TimeSpan.Zero)
                {
                    await Task.Delay(tipShowDelay);
                }

                if (!_contentFinishedLoading)
                {
                    TipTextLoad = true;
                    TipText = text;
                }
            }
            else
            {
                TipTextLoad = false;
            }
        }

        #endregion

        #region Private Helpers

        private void ShowOrHideCanvasLoadingProgress(bool show, BaseContentTypeModel contentType)
        {
            if (show)
            {
                if (contentType is InfiniteCanvasContentType)
                {
                    InfiniteCanvasProgressLoad = true;
                    CanvasRingLoad = false;
                }
                else
                {
                    InfiniteCanvasProgressLoad = false;
                    CanvasRingLoad = true;
                }
            }
            else
            {
                InfiniteCanvasProgressLoad = false;
                CanvasRingLoad = false;
            }
        }

        private async Task PrepareForContentStartLoading(BaseContentTypeModel contentType)
        {
            _contentFinishedLoading = false;
            NewCanvasScreenLoad = false;
            TipTextLoad = false;
            ShowOrHideCanvasLoadingProgress(false, contentType);

            // Await a short delay before showing the loading ring
            await Task.Delay(Constants.UI.CanvasContent.SHOW_LOADING_RING_DELAY);

            if (!_contentFinishedLoading) // The value might have changed
            {
                ShowOrHideCanvasLoadingProgress(true, contentType);
            }
        }

        private async Task PasteDataInternal(DataPackageView dataPackage = null)
        {
            SafeWrapperResult result = null;
            BaseReadOnlyCanvasPreviewControlViewModel<BaseReadOnlyCanvasViewModel>.CanvasPasteCancellationTokenSource.Cancel();
            BaseReadOnlyCanvasPreviewControlViewModel<BaseReadOnlyCanvasViewModel>.CanvasPasteCancellationTokenSource = new CancellationTokenSource();

            if (dataPackage == null)
            {
                SafeWrapper<DataPackageView> dataPackageWrapper = ClipboardHelpers.GetClipboardData();

                if (dataPackageWrapper)
                {
                    result = await PasteCanvasModel.TryPasteData(dataPackageWrapper, CanvasPreviewControlViewModel.CanvasPasteCancellationTokenSource.Token);
                }
            }
            else
            {
                result = await PasteCanvasModel.TryPasteData(dataPackage, CanvasPreviewControlViewModel.CanvasPasteCancellationTokenSource.Token);
            }

            if (result.Exception is ObjectDisposedException)
            {
                // The last instance was disposed
                // That's probably because user tried pasting data to a filled canvas, so new canvas was opened and this disposed
                return;
            }
            else if (!result) // Check if anything went wrong
            {
                //Debugger.Break(); TODO: Uncomment later
            }
        }

        #endregion

        #region Event Hooks

        private void HookEvents()
        {
            UnhookEvents();
            if (PasteCanvasModel != null)
            {
                PasteCanvasModel.OnPasteInitiatedEvent += PasteCanvasModel_OnPasteInitiatedEvent;
                PasteCanvasModel.OnContentStartedLoadingEvent += PasteCanvasModel_OnContentStartedLoadingEvent;
                PasteCanvasModel.OnContentLoadedEvent += PasteCanvasModel_OnContentLoadedEvent;
                PasteCanvasModel.OnContentLoadFailedEvent += PasteCanvasModel_OnContentLoadFailedEvent;
                PasteCanvasModel.OnErrorOccurredEvent += PasteCanvasModel_OnErrorOccurredEvent;
                PasteCanvasModel.OnProgressReportedEvent += PasteCanvasModel_OnProgressReportedEvent;
                PasteCanvasModel.OnTipTextUpdateRequestedEvent += PasteCanvasModel_OnTipTextUpdateRequestedEvent;
            }
        }

        private void UnhookEvents()
        {
            if (PasteCanvasModel != null)
            {
                PasteCanvasModel.OnPasteInitiatedEvent -= PasteCanvasModel_OnPasteInitiatedEvent;
                PasteCanvasModel.OnContentStartedLoadingEvent -= PasteCanvasModel_OnContentStartedLoadingEvent;
                PasteCanvasModel.OnContentLoadedEvent -= PasteCanvasModel_OnContentLoadedEvent;
                PasteCanvasModel.OnContentLoadFailedEvent -= PasteCanvasModel_OnContentLoadFailedEvent;
                PasteCanvasModel.OnErrorOccurredEvent -= PasteCanvasModel_OnErrorOccurredEvent;
                PasteCanvasModel.OnProgressReportedEvent -= PasteCanvasModel_OnProgressReportedEvent;
                PasteCanvasModel.OnTipTextUpdateRequestedEvent -= PasteCanvasModel_OnTipTextUpdateRequestedEvent;
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            UnhookEvents();
            PasteCanvasModel?.Dispose();
        }

        #endregion
    }
}
