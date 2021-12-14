using System;
using System.Windows.Input;
using Windows.System;
using Microsoft.UI.Xaml.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ClipboardCanvas.Enums;
using ClipboardCanvas.Models;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class NavigationControlViewModel : ObservableObject, INavigationControlModel
    {
        #region Private Members

        private DisplayPageType _currentPage;

        #endregion

        #region Public Properties

        private bool _CanvasLayoutControlsLoad;
        public bool CanvasLayoutControlsLoad
        {
            get => _CanvasLayoutControlsLoad;
            set => SetProperty(ref _CanvasLayoutControlsLoad, value);
        }

        private bool _HomepageLayoutControlsLoad;
        public bool HomepageLayoutControlsLoad
        {
            get => _HomepageLayoutControlsLoad;
            set => SetProperty(ref _HomepageLayoutControlsLoad, value);
        }

        private bool _CollectionPreviewLayoutControlsLoad;
        public bool CollectionPreviewLayoutControlsLoad
        {
            get => _CollectionPreviewLayoutControlsLoad;
            set => SetProperty(ref _CollectionPreviewLayoutControlsLoad, value);
        }

        private bool _NavigateBackEnabled = true;
        public bool NavigateBackEnabled
        {
            get => _NavigateBackEnabled;
            set => SetProperty(ref _NavigateBackEnabled, value);
        }

        private bool _NavigateBackLoading = false;
        public bool NavigateBackLoading
        {
            get => _NavigateBackLoading;
            set => SetProperty(ref _NavigateBackLoading, value);
        }

        private bool _NavigateForwardEnabled = true;
        public bool NavigateForwardEnabled
        {
            get => _NavigateForwardEnabled;
            set => SetProperty(ref _NavigateForwardEnabled, value);
        }

        private bool _NavigateForwardLoading = false;
        public bool NavigateForwardLoading
        {
            get => _NavigateForwardLoading;
            set => SetProperty(ref _NavigateForwardLoading, value);
        }

        private bool _GoToCanvasEnabled = true;
        public bool GoToCanvasEnabled
        {
            get => _GoToCanvasEnabled;
            set => SetProperty(ref _GoToCanvasEnabled, value);
        }

        private bool _CollectionPreviewGoToCanvasEnabled = true;
        public bool CollectionPreviewGoToCanvasEnabled
        {
            get => _CollectionPreviewGoToCanvasEnabled;
            set => SetProperty(ref _CollectionPreviewGoToCanvasEnabled, value);
        }

        private bool _CollectionPreviewGoToCanvasLoading;
        public bool CollectionPreviewGoToCanvasLoading
        {
            get => _CollectionPreviewGoToCanvasLoading;
            set => SetProperty(ref _CollectionPreviewGoToCanvasLoading, value);
        }

        #endregion

        #region Events

        public event EventHandler OnNavigateLastRequestedEvent;

        public event EventHandler OnNavigateBackRequestedEvent;

        public event EventHandler OnNavigateFirstRequestedEvent;

        public event EventHandler OnNavigateForwardRequestedEvent;

        public event EventHandler OnGoToHomepageRequestedEvent;

        public event EventHandler OnGoToCanvasRequestedEvent;

        public event EventHandler OnCollectionPreviewNavigateBackRequestedEvent;

        public event EventHandler OnCollectionPreviewGoToCanvasRequestedEvent;

        #endregion

        #region Commands

        public ICommand NavigateLastCommand { get; private set; }

        public ICommand NavigateBackCommand { get; private set; }

        public ICommand NavigateFirstCommand { get; private set; }

        public ICommand NavigateForwardCommand { get; private set; }

        public ICommand GoToHomepageCommand { get; private set; }

        public ICommand GoToCanvasCommand { get; private set; }

        public ICommand CollectionPreviewNavigateBackCommand { get; private set; }

        public ICommand CollectionPreviewGoToCanvasCommand { get; private set; }

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
            GoToHomepageCommand = new RelayCommand(GoToHomepage);
            GoToCanvasCommand = new RelayCommand(GoToCanvas);
            CollectionPreviewNavigateBackCommand = new RelayCommand(CollectionPreviewNavigateBack);
            CollectionPreviewGoToCanvasCommand = new RelayCommand(CollectionPreviewGoToCanvas);
            DefaultKeyboardAcceleratorInvokedCommand = new RelayCommand<KeyboardAcceleratorInvokedEventArgs>(DefaultKeyboardAcceleratorInvoked);
        }

        #endregion

        #region Command Implementation

        private void DefaultKeyboardAcceleratorInvoked(KeyboardAcceleratorInvokedEventArgs e)
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
                        if (_currentPage == DisplayPageType.Homepage)
                        {
                            break;
                        }

                        GoToHomepage();
                        break;
                    }

                case (c: false, s: false, a: false, w: false, k: VirtualKey.Right):
                    {
                        NavigateForward();
                        break;
                    }

                case (c: false, s: false, a: false, w: false, k: VirtualKey.Home):
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

                case (c: false, s: false, a: false, w: false, k: VirtualKey.End):
                    {
                        NavigateLast();
                        break;
                    }

                case (c: true, s: false, a: false, w: false, k: VirtualKey.Left):
                    {
                        if (_currentPage == DisplayPageType.CanvasPage)
                        {
                            NavigateLast();
                        }
                        else
                        {
                            CollectionPreviewNavigateBack();
                        }

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

        private void GoToHomepage()
        {
            OnGoToHomepageRequestedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void GoToCanvas()
        {
            OnGoToCanvasRequestedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void CollectionPreviewNavigateBack()
        {
            OnCollectionPreviewNavigateBackRequestedEvent?.Invoke(this, EventArgs.Empty);
        }

        private void CollectionPreviewGoToCanvas()
        {
            OnCollectionPreviewGoToCanvasRequestedEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region INavigationToolBarControlModel

        public void NotifyCurrentPageChanged(DisplayPageType pageType)
        {
            // TODO: Instead of checking it there, hook up INavigationService event
            _currentPage = pageType;

            switch (_currentPage)
            {
                case DisplayPageType.Homepage:
                    {
                        ChangeButtonLayoutOnHome();
                        break;
                    }

                case DisplayPageType.CanvasPage:
                    {
                        ChangeButtonLayoutOnCanvas();
                        break;
                    }

                case DisplayPageType.CollectionPreviewPage:
                    {
                        ChangeButtonLayoutOnCollectionPreview();
                        break;
                    }
            }
        }

        #endregion

        #region Private Helpers

        private void ChangeButtonLayoutOnCanvas()
        {
            CanvasLayoutControlsLoad = true;
            HomepageLayoutControlsLoad = false;
            CollectionPreviewLayoutControlsLoad = false;
        }

        private void ChangeButtonLayoutOnHome()
        {
            CanvasLayoutControlsLoad = false;
            HomepageLayoutControlsLoad = true;
            CollectionPreviewLayoutControlsLoad = false;
        }

        private void ChangeButtonLayoutOnCollectionPreview()
        {
            CanvasLayoutControlsLoad = false;
            HomepageLayoutControlsLoad = false;
            CollectionPreviewLayoutControlsLoad = true;
        }

        #endregion
    }
}
