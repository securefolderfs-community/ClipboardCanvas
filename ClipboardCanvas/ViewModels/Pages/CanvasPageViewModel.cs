using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using System.Threading.Tasks;
using ClipboardCanvas.EventArguments.CanvasControl;
using Windows.UI.Xaml;
using System.Collections.Generic;
using Windows.Storage;
using System.Linq;
using ClipboardCanvas.Enums;

namespace ClipboardCanvas.ViewModels.Pages
{
    public class CanvasPageViewModel : ObservableObject, IPasteCanvasPageModel, IDisposable
    {
        #region Private Members

        private readonly ICanvasPageView _view;

        private const string DEFAULT_TITLE_TEXT = "Press Ctrl+V to paste in content!";

        private const string DRAG_TITLE_TEXT = "Release to paste in content!";

        private bool _contentLoaded;

        #endregion

        #region Public Properties

        public ICollectionsContainerModel CollectionContainer => _view?.AssociatedCollection;

        public IPasteCanvasModel PasteCanvasModel => _view?.PasteCanvasModel;

        private bool _IsDragAvailable = true;
        public bool IsDragAvailable
        {
            get => _IsDragAvailable;
            private set => SetProperty(ref _IsDragAvailable, value);
        }

        private bool _TitleTextLoad = true;
        public bool TitleTextLoad
        {
            get => _TitleTextLoad;
            set => SetProperty(ref _TitleTextLoad, value);
        }

        private bool _CanvasRingLoad = false;
        public bool CanvasRingLoad
        {
            get => _CanvasRingLoad;
            set => SetProperty(ref _CanvasRingLoad, value);
        }

        private string _TitleText = DEFAULT_TITLE_TEXT;
        public string TitleText
        {
            get => _TitleText;
            set => SetProperty(ref _TitleText, value);
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

        private bool _OperationProgressBarLoad;
        public bool OperationProgressBarLoad
        {
            get => _OperationProgressBarLoad;
            set => SetProperty(ref _OperationProgressBarLoad, value);
        }

        private bool _OperationProgressBarIndeterminate;
        public bool OperationProgressBarIndeterminate
        {
            get => _OperationProgressBarIndeterminate;
            set => SetProperty(ref _OperationProgressBarIndeterminate, value);
        }

        private float _OperationProgressBarValue;
        public float OperationProgressBarValue
        {
            get => _OperationProgressBarValue;
            set => SetProperty(ref _OperationProgressBarValue, value);
        }

        #endregion

        #region Commands

        public ICommand DefaultKeyboardAcceleratorInvokedCommand { get; private set; }

        public ICommand DragEnterCommand { get; private set; }

        public ICommand DragLeaveCommand { get; private set; }

        public ICommand DropCommand { get; private set; }

        public ICommand OverrideReferenceCommand { get; private set; }

        #endregion

        #region Constructor

        public CanvasPageViewModel(ICanvasPageView view)
        {
            this._view = view;

            HookEvents();

            // Create commands
            DefaultKeyboardAcceleratorInvokedCommand = new RelayCommand<KeyboardAcceleratorInvokedEventArgs>(DefaultKeyboardAcceleratorInvoked);
            DragEnterCommand = new RelayCommand<DragEventArgs>(DragEnter);
            DragLeaveCommand = new RelayCommand<DragEventArgs>(DragLeave);
            DropCommand = new RelayCommand<DragEventArgs>(Drop);
            OverrideReferenceCommand = new RelayCommand(OverrideReference);
        }

        #endregion

        #region Command Implementation

        private async void DefaultKeyboardAcceleratorInvoked(KeyboardAcceleratorInvokedEventArgs e)
        {
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
            }
        }

        private async void DragEnter(DragEventArgs e)
        {
            DragOperationDeferral deferral = null;

            try
            {
                deferral = e.GetDeferral();

                SafeWrapper<IReadOnlyList<IStorageItem>> draggedItems = await SafeWrapperRoutines.SafeWrapAsync(async () =>
                    await e.DataView.GetStorageItemsAsync());

                if (draggedItems)
                {
                    if (CollectionContainer.IsOnNewCanvas || (!CollectionContainer.IsOnNewCanvas && !draggedItems.Result.Any((item) => item.Path == CollectionContainer.CurrentCanvas.File.Path)))
                    {
                        e.AcceptedOperation = DataPackageOperation.Copy;
                        TitleText = DRAG_TITLE_TEXT;
                    }
                    else
                    {
                        e.AcceptedOperation = DataPackageOperation.None;
                        TitleText = DEFAULT_TITLE_TEXT;
                    }
                }
                else
                {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                    TitleText = DRAG_TITLE_TEXT;
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

        private async void Drop(DragEventArgs e)
        {
            DataPackageView dataPackage = e.DataView;
            await PasteDataInternal(dataPackage);
        }

        private async void OverrideReference()
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

        #endregion

        #region Event Handlers

        private void PasteCanvasModel_OnProgressReportedEvent(object sender, ProgressReportedEventArgs e)
        {
            switch (e.progressType)
            {
                case CanvasPageProgressType.MainCanvasProgressBar:
                    {
                        if (e.value == 100.0f)
                        {
                            CanvasRingLoad = false;
                        }
                        else
                        {
                            CanvasRingLoad = true;
                        }
                        break;
                    }

                case CanvasPageProgressType.OperationProgressBar:
                    {
                        if (e.value >= 100.0f)
                        {
                            OperationProgressBarIndeterminate = false;
                            OperationProgressBarLoad = false;
                        }
                        else
                        {
                            OperationProgressBarLoad = true;
                            OperationProgressBarValue = e.value;

                            if (OperationProgressBarValue == 0.0f || e.isIndeterminate)
                            {
                                OperationProgressBarIndeterminate = true;
                            }
                            else
                            {
                                OperationProgressBarIndeterminate = false;
                            }
                        }

                        break;
                    }
            }
        }

        private void PasteCanvasModel_OnErrorOccurredEvent(object sender, ErrorOccurredEventArgs e)
        {
            ErrorTextLoad = true;
            ErrorText = e.errorMessage;

            if (e.showErrorImage)
            {
                TitleTextLoad = false;
                TipTextLoad = false;
                PasteCanvasModel?.DiscardData();
            }
        }

        private async void PasteCanvasModel_OnContentStartedLoadingEvent(object sender, ContentStartedLoadingEventArgs e)
        {
            await PrepareForContentStartLoading();
        }

        private void PasteCanvasModel_OnContentLoadedEvent(object sender, ContentLoadedEventArgs e)
        {
            PastedAsReferenceLoad = e.pastedByReference;
            CanvasRingLoad = false;
            _contentLoaded = true;
            TitleTextLoad = false;
            TipTextLoad = false;
            ErrorTextLoad = false;
        }

        private async void PasteCanvasModel_OnPasteRequestedEvent(object sender, PasteRequestedEventArgs e)
        {
            await PrepareForContentStartLoading();
        }

        #endregion

        #region IPasteCanvasPageModel

        public void SetTipText(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                TipTextLoad = true;
                TipText = text;
            }
            else
            {
                TipTextLoad = false;
            }
        }

        #endregion

        #region Private Helpers

        private async Task PrepareForContentStartLoading()
        {
            _contentLoaded = false;
            TitleTextLoad = false;
            TipTextLoad = false;

            // Await a short delay before showing the loading ring
            await Task.Delay(Constants.CanvasContent.SHOW_LOADING_RING_AFTER_TIME);

            if (!_contentLoaded) // The value might have changed
            {
                CanvasRingLoad = true;
            }
        }

        private async Task PasteDataInternal(DataPackageView dataPackage = null)
        {
            SafeWrapperResult result = null;
            DynamicCanvasControlViewModel.CanvasPasteCancellationTokenSource.Cancel();
            DynamicCanvasControlViewModel.CanvasPasteCancellationTokenSource = new CancellationTokenSource();

            if (dataPackage == null)
            {
                SafeWrapper<DataPackageView> dataPackageWrapper = await ClipboardHelpers.GetClipboardData();

                if (dataPackageWrapper)
                {
                    result = await PasteCanvasModel.TryPasteData(dataPackageWrapper, DynamicCanvasControlViewModel.CanvasPasteCancellationTokenSource.Token);
                }
            }
            else
            {
                result = await PasteCanvasModel.TryPasteData(dataPackage, DynamicCanvasControlViewModel.CanvasPasteCancellationTokenSource.Token);
            }

            if (result == null)
            {
                // The last instance was probably disposed
                // That's probably because user tried pasting data to a filled canvas, so new canvas was opened and this disposed
                return;
            }
            else if (!result) // Check if anything went wrong
            {
                Debugger.Break();
            }
        }

        #endregion

        #region Event Hooks

        private void HookEvents()
        {
            UnhookEvents();
            if (PasteCanvasModel != null)
            {
                PasteCanvasModel.OnPasteRequestedEvent += PasteCanvasModel_OnPasteRequestedEvent;
                PasteCanvasModel.OnContentLoadedEvent += PasteCanvasModel_OnContentLoadedEvent;
                PasteCanvasModel.OnContentStartedLoadingEvent += PasteCanvasModel_OnContentStartedLoadingEvent;
                PasteCanvasModel.OnErrorOccurredEvent += PasteCanvasModel_OnErrorOccurredEvent;
                PasteCanvasModel.OnProgressReportedEvent += PasteCanvasModel_OnProgressReportedEvent;
            }
        }

        private void UnhookEvents()
        {
            if (PasteCanvasModel != null)
            {
                PasteCanvasModel.OnPasteRequestedEvent -= PasteCanvasModel_OnPasteRequestedEvent;
                PasteCanvasModel.OnContentLoadedEvent -= PasteCanvasModel_OnContentLoadedEvent;
                PasteCanvasModel.OnContentStartedLoadingEvent -= PasteCanvasModel_OnContentStartedLoadingEvent;
                PasteCanvasModel.OnErrorOccurredEvent -= PasteCanvasModel_OnErrorOccurredEvent;
                PasteCanvasModel.OnProgressReportedEvent -= PasteCanvasModel_OnProgressReportedEvent;
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
