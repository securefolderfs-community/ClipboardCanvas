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

namespace ClipboardCanvas.ViewModels.Pages
{
    public class CanvasPageViewModel : ObservableObject, IDisposable
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

        private bool _TitleTextLoad = true;
        public bool TitleTextLoad
        {
            get => _TitleTextLoad;
            set => SetProperty(ref _TitleTextLoad, value);
        }

        private bool _LoadingRingLoad = false;
        public bool LoadingRingLoad
        {
            get => _LoadingRingLoad;
            set => SetProperty(ref _LoadingRingLoad, value);
        }

        private string _TitleText = DEFAULT_TITLE_TEXT;
        public string TitleText
        {
            get => _TitleText;
            set => SetProperty(ref _TitleText, value);
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

        private bool _ProgressBarLoad;
        public bool ProgressBarLoad
        {
            get => _ProgressBarLoad;
            set => SetProperty(ref _ProgressBarLoad, value);
        }

        private bool _ProgressBarIndeterminate;
        public bool ProgressBarIndeterminate
        {
            get => _ProgressBarIndeterminate;
            set => SetProperty(ref _ProgressBarIndeterminate, value);
        }

        private float _ProgressBarValue;
        public float ProgressBarValue
        {
            get => _ProgressBarValue;
            set => SetProperty(ref _ProgressBarValue, value);
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

        private void DragEnter(DragEventArgs e)
        {
            DragOperationDeferral deferral = null;

            try
            {
                deferral = e.GetDeferral();

                e.AcceptedOperation = DataPackageOperation.Copy;
                TitleText = DRAG_TITLE_TEXT;
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
            ProgressBarLoad = true;
            ProgressBarValue = e.progress;

            if (ProgressBarValue == 0.0f)
            {
                ProgressBarIndeterminate = true;
            }
            else if (e.progress >= 100.0f)
            {
                // TODO: Have smooth fade-out animation for hiding the progressbar
                ProgressBarIndeterminate = false;
                ProgressBarLoad = false;
            }
            else
            {
                ProgressBarIndeterminate = false;
            }
        }

        private void PasteCanvasModel_OnErrorOccurredEvent(object sender, ErrorOccurredEventArgs e)
        {
            ErrorTextLoad = true;
            ErrorText = e.errorMessage;

            if (e.showErrorImage)
            {
                TitleTextLoad = false;
                PasteCanvasModel?.DiscardData();
            }
        }

        private async void PasteCanvasModel_OnContentStartedLoadingEvent(object sender, ContentStartedLoadingEventArgs e)
        {
            _contentLoaded = false;
            TitleTextLoad = false;

            // Await a short delay before showing the loading ring
            await Task.Delay(Constants.CanvasContent.SHOW_LOADING_RING_AFTER_TIME);

            if (!_contentLoaded) // The value might have changed
            {
                LoadingRingLoad = true;
            }
        }

        private void PasteCanvasModel_OnContentLoadedEvent(object sender, ContentLoadedEventArgs e)
        {
            PastedAsReferenceLoad = e.pastedByReference;
            LoadingRingLoad = false;
            _contentLoaded = true;
            TitleTextLoad = false;
            ErrorTextLoad = false;
        }

        #endregion

        #region Private Helpers

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
        }

        #endregion
    }
}
