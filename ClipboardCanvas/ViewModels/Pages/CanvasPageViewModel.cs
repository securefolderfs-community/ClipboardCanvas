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

namespace ClipboardCanvas.ViewModels.Pages
{
    public class CanvasPageViewModel : ObservableObject, IDisposable
    {
        #region Private Members

        private readonly ICanvasPageView _view;

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

        private bool _ProgressBarLoad;
        public bool ProgressBarLoad
        {
            get => _ProgressBarLoad;
            set => SetProperty(ref _ProgressBarLoad, value);
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

        #endregion

        #region Constructor

        public CanvasPageViewModel(ICanvasPageView view)
        {
            this._view = view;

            HookEvents();

            // Create commands
            DefaultKeyboardAcceleratorInvokedCommand = new RelayCommand<KeyboardAcceleratorInvokedEventArgs>(DefaultKeyboardAcceleratorInvoked);
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
                        DynamicCanvasControlViewModel.CanvasPasteCancellationTokenSource.Cancel();
                        DynamicCanvasControlViewModel.CanvasPasteCancellationTokenSource = new CancellationTokenSource();
                        SafeWrapper<DataPackageView> dataPackage = await ClipboardHelpers.GetClipboardData();
                        SafeWrapperResult result = dataPackage;

                        if (result)
                        {
                            result = await PasteCanvasModel.TryPasteData(dataPackage, DynamicCanvasControlViewModel.CanvasPasteCancellationTokenSource.Token);
                        }

                        // Check if anything went wrong
                        if (!result)
                        {
                            Debugger.Break();
                        }

                        break;
                    }
            }
        }

        #endregion

        #region Event Handlers

        private async void PasteCanvasModel_OnProgressReportedEvent(object sender, ProgressReportedEventArgs e)
        {
            ProgressBarLoad = true;
            ProgressBarValue = e.progress;

            if (e.progress >= 100.0f)
            {
                // TODO: Have smooth fade-out animation for hiding the progressbar

                await Task.Delay(1500); // bruh

                ProgressBarLoad = false;
            }
        }

        private void PasteCanvasModel_OnErrorOccurredEvent(object sender, ErrorOccurredEventArgs e)
        {
            ErrorTextLoad = true;
            ErrorText = e.errorMessage;

            if (e.showErrorImage)
            {
                PasteCanvasModel?.DiscardData();
            }
        }

        private void PasteCanvasModel_OnContentLoadedEvent(object sender, ContentLoadedEventArgs e)
        {
            PastedAsReferenceLoad = e.pastedByReference;
            TitleTextLoad = false;
            ErrorTextLoad = false;
        }

        #endregion

        #region Event Hooks

        private void HookEvents()
        {
            UnhookEvents();
            if (PasteCanvasModel != null)
            {
                PasteCanvasModel.OnContentLoadedEvent += PasteCanvasModel_OnContentLoadedEvent;
                PasteCanvasModel.OnErrorOccurredEvent += PasteCanvasModel_OnErrorOccurredEvent;
                PasteCanvasModel.OnProgressReportedEvent += PasteCanvasModel_OnProgressReportedEvent;
            }
        }

        private void UnhookEvents()
        {
            if (PasteCanvasModel != null)
            {
                PasteCanvasModel.OnContentLoadedEvent -= PasteCanvasModel_OnContentLoadedEvent;
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
