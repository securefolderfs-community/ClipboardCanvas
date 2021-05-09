using ClipboardCanvas.DataModels;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Xaml.Input;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class NavigationControlViewModel : ObservableObject, INavigationControlModel
    {
        #region Private Members

        private DisplayPageType _currentPage;

        #endregion

        #region Public Properties

        private bool _NavigateBackLoad;
        public bool NavigateBackLoad
        {
            get => _NavigateBackLoad;
            private set => SetProperty(ref _NavigateBackLoad, value);
        }

        private bool _NavigateForwardLoad;
        public bool NavigateForwardLoad
        {
            get => _NavigateForwardLoad;
            private set => SetProperty(ref _NavigateForwardLoad, value);
        }

        private bool _NavigateBackEnabled = true;
        public bool NavigateBackEnabled
        {
            get => _NavigateBackEnabled;
            set => SetProperty(ref _NavigateBackEnabled, value);
        }

        private bool _NavigateForwardEnabled = true;
        public bool NavigateForwardEnabled
        {
            get => _NavigateForwardEnabled;
            set => SetProperty(ref _NavigateForwardEnabled, value);
        }

        private bool _GoToHomeLoad;
        public bool GoToHomeLoad
        {
            get => _GoToHomeLoad;
            private set => SetProperty(ref _GoToHomeLoad, value);
        }

        private bool _GoToCanvasLoad;
        public bool GoToCanvasLoad
        {
            get => _GoToCanvasLoad;
            private set => SetProperty(ref _GoToCanvasLoad, value);
        }

        #endregion

        #region Events

        public event EventHandler OnNavigateLastRequestedEvent;

        public event EventHandler OnNavigateBackRequestedEvent;

        public event EventHandler OnNavigateFirstRequestedEvent;

        public event EventHandler OnNavigateForwardRequestedEvent;

        public event EventHandler OnGoToHomePageRequestedEvent;

        public event EventHandler OnGoToCanvasRequestedEvent;

        #endregion

        #region Commands

        public ICommand NavigateLastCommand { get; private set; }

        public ICommand NavigateBackCommand { get; private set; }

        public ICommand NavigateFirstCommand { get; private set; }

        public ICommand NavigateForwardCommand { get; private set; }

        public ICommand GoToHomeCommand { get; private set; }

        public ICommand GoToCanvasCommand { get; private set; }

        public ICommand DefaultKeyboardAcceleratorInvokedCommand { get; private set; }

        #endregion

        #region Constructor

        public NavigationControlViewModel()
        {
            ChangeButtonLayoutOnCanvas();

            // Create commands
            NavigateLastCommand = new RelayCommand(NavigateLast);
            NavigateBackCommand = new RelayCommand(NavigateBack);
            NavigateFirstCommand = new RelayCommand(NavigateFirst);
            NavigateForwardCommand = new RelayCommand(NavigateForward);
            GoToHomeCommand = new RelayCommand(GoToHome);
            GoToCanvasCommand = new RelayCommand(GoToCanvas);
            DefaultKeyboardAcceleratorInvokedCommand = new RelayCommand<KeyboardAcceleratorInvokedEventArgs>(DefaultKeyboardAcceleratorInvoked);
        }

        #endregion

        #region Command Implementation

        private void DefaultKeyboardAcceleratorInvoked(KeyboardAcceleratorInvokedEventArgs e)
        {
            bool ctrl = e.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Control);
            bool shift = e.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Shift);
            bool alt = e.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Menu);
            bool win = e.KeyboardAccelerator.Modifiers.HasFlag(VirtualKeyModifiers.Windows);
            VirtualKey vkey = e.KeyboardAccelerator.Key;
            uint uVkey = (uint)e.KeyboardAccelerator.Key;

            switch (c: ctrl, s: shift, a: alt, w: win, k: vkey)
            {
                case (c: true, s: false, a: false, w: false, k: VirtualKey.Up):
                    {
                        if (_currentPage == DisplayPageType.CanvasPage)
                        {
                            break;
                        }

                        GoToCanvas();
                        break;
                    }

                case (c: true, s: false, a: false, w: false, k: VirtualKey.Down):
                    {
                        if (_currentPage == DisplayPageType.HomePage)
                        {
                            break;
                        }

                        GoToHome();
                        break;
                    }

                case (c: false, s: false, a: false, w: false, k: VirtualKey.Right):
                    {
                        NavigateForward();
                        break;
                    }

                case (c: true, s: false, a: false, w: false, k: VirtualKey.Right):
                    {
                        NavigateFirst();
                        break;
                    }

                case (c: false, s: false, a: false, w: false, k: VirtualKey.Left):
                    {
                        NavigateBack();
                        break;
                    }

                case (c: true, s: false, a: false, w: false, k: VirtualKey.Left):
                    {
                        NavigateLast();
                        break;
                    }
            }
        }

        private void NavigateLast()
        {
            OnNavigateLastRequestedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void NavigateBack()
        {
            OnNavigateBackRequestedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void NavigateFirst()
        {
            OnNavigateFirstRequestedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void NavigateForward()
        {
            OnNavigateForwardRequestedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void GoToHome()
        {
            OnGoToHomePageRequestedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void GoToCanvas()
        {
            OnGoToCanvasRequestedEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IInstanceNotifyModel

        public void NotifyCurrentPageChanged(DisplayFrameNavigationDataModel navigationDataModel)
        {
            _currentPage = navigationDataModel.pageType;
            switch (navigationDataModel.pageType)
            {
                case DisplayPageType.HomePage:
                    {
                        ChangeButtonLayoutOnHome();
                        break;
                    }

                case DisplayPageType.CanvasPage:
                    {
                        ChangeButtonLayoutOnCanvas();
                        break;
                    }

                case DisplayPageType.CollectionsPreview:
                    {
                        Debugger.Break();
                        break;
                    }
            }
        }

        #endregion

        #region Private Helpers

        private void ChangeButtonLayoutOnCanvas()
        {
            NavigateBackLoad = true;
            NavigateForwardLoad = true;
            GoToHomeLoad = true;
            GoToCanvasLoad = false;
        }

        private void ChangeButtonLayoutOnHome()
        {
            NavigateBackLoad = false;
            NavigateForwardLoad = false;
            GoToHomeLoad = false;
            GoToCanvasLoad = true;
        }

        #endregion
    }
}
