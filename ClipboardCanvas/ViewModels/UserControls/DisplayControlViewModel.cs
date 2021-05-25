using System;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Threading;
using System.Diagnostics;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.Enums;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Extensions;
using Windows.UI.ViewManagement;
using System.Collections;
using System.Collections.Generic;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.EventArguments.CollectionControl;
using Windows.UI.Xaml.Media.Animation;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class DisplayControlViewModel : ObservableObject, IDisposable
    {
        #region Private Members

        private readonly IDisplayControlView _view;

        private ICollectionsContainerModel _currentCollectionContainer;

        private CancellationTokenSource _canvasLoadCancellationTokenSource;

        private IWindowTitleBarControlModel WindowTitleBarControlModel => _view?.WindowTitleBarControlModel;

        private INavigationToolBarControlModel NavigationToolBarControlModel => _view?.NavigationToolBarControlModel;

        /// <summary>
        /// Stores canvas control
        /// <br/><br/>
        /// Note: This might be null depending on the current view
        /// </summary>
        private IPasteCanvasPageModel PasteCanvasPageModel => _view?.PasteCanvasPageModel;

        #endregion

        #region Public Properties

        private DisplayFrameNavigationDataModel _CurrentPageNavigation;
        public DisplayFrameNavigationDataModel CurrentPageNavigation
        {
            get => _CurrentPageNavigation;
            private set
            {
                // Page switch requested so we need to unhook events
                if (_CurrentPageNavigation != null)
                {
                    UnhookCanvasControlEvents(); // Unhook events
                    PasteCanvasPageModel?.Dispose(); // Dispose stuff
                }

                if (SetProperty(ref _CurrentPageNavigation, value))
                {
                    _CurrentPageNavigation.simulateNavigation = false;
                    NavigationToolBarControlModel?.NotifyCurrentPageChanged(value);

                    // Hook events if the page navigated is canvas page
                    if (_CurrentPageNavigation != null && _CurrentPageNavigation.pageType == DisplayPageType.CanvasPage)
                    {
                        HookCanvasControlEvents();
                    }

                    OnPageNavigated(_CurrentPageNavigation);
                }
            }
        }

        public DisplayPageType CurrentPage => CurrentPageNavigation.pageType;

        #endregion

        #region Constructor

        public DisplayControlViewModel(IDisplayControlView view)
        {
            this._view = view;
            this._canvasLoadCancellationTokenSource = new CancellationTokenSource();

            HookCollectionsEvents();
        }

        #endregion

        #region Event Handlers

        #region WindowTitleBarControlModel

        private async void WindowTitleBarControlModel_OnSwitchApplicationViewRequestedEvent(object sender, EventArgs e)
        {
            if (ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.Default)
            {
                if (await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay))
                {
                    UpdateViewForCompactOverlayMode();
                }
            }
            else
            {
                if (await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default))
                {
                    UpdateViewForDefaultMode();
                }
            }
        }

        #endregion

        #region NavigationControlModel

        private async void NavigationControlModel_OnNavigateLastRequestedEvent(object sender, EventArgs e)
        {
            if (CurrentPage == DisplayPageType.CanvasPage && _currentCollectionContainer.HasBack())
            {
                _canvasLoadCancellationTokenSource.Cancel();
                _canvasLoadCancellationTokenSource.Dispose();
                _canvasLoadCancellationTokenSource = new CancellationTokenSource();

                await _currentCollectionContainer.NavigateLast(PasteCanvasPageModel.PasteCanvasModel, _canvasLoadCancellationTokenSource.Token);
                CheckNavigation();
                await SetSuggestedActions();
            }
        }

        private async void NavigationControlModel_OnNavigateBackRequestedEvent(object sender, EventArgs e)
        {
            if (CurrentPage == DisplayPageType.CanvasPage && _currentCollectionContainer.HasBack())
            {
                _canvasLoadCancellationTokenSource.Cancel();
                _canvasLoadCancellationTokenSource.Dispose();
                _canvasLoadCancellationTokenSource = new CancellationTokenSource();

                await _currentCollectionContainer.NavigateBack(PasteCanvasPageModel.PasteCanvasModel, _canvasLoadCancellationTokenSource.Token);
                CheckNavigation();
                await SetSuggestedActions();
            }
        }

        private async void NavigationControlModel_OnNavigateFirstRequestedEvent(object sender, EventArgs e)
        {
            if (CurrentPage == DisplayPageType.CanvasPage && _currentCollectionContainer.HasNext())
            {
                _canvasLoadCancellationTokenSource.Cancel();
                _canvasLoadCancellationTokenSource.Dispose();
                _canvasLoadCancellationTokenSource = new CancellationTokenSource();

                _currentCollectionContainer.NavigateFirst(PasteCanvasPageModel.PasteCanvasModel);
                CheckNavigation();
                await SetSuggestedActions();
            }
        }

        private async void NavigationControlModel_OnNavigateForwardRequestedEvent(object sender, EventArgs e)
        {
            if (CurrentPage == DisplayPageType.CanvasPage && _currentCollectionContainer.HasNext())
            {
                _canvasLoadCancellationTokenSource.Cancel();
                _canvasLoadCancellationTokenSource.Dispose();
                _canvasLoadCancellationTokenSource = new CancellationTokenSource();

                await _currentCollectionContainer.NavigateNext(PasteCanvasPageModel.PasteCanvasModel, _canvasLoadCancellationTokenSource.Token);
                CheckNavigation();
                await SetSuggestedActions();
            }
        }

        private async void NavigationControlModel_OnGoToHomePageRequestedEvent(object sender, EventArgs e)
        {
            await OpenPage(DisplayPageType.HomePage);
        }

        private async void NavigationControlModel_OnGoToCanvasRequestedEvent(object sender, EventArgs e)
        {
            await OpenPage(DisplayPageType.CanvasPage);
        }

        #endregion

        #region CollectionsControlViewModel

        private void CollectionsControlViewModel_OnCollectionErrorRaisedEvent(object sender, CollectionErrorRaisedEventArgs e)
        {
            if (e.result)
            {
                NavigationToolBarControlModel.NavigationControlModel.GoToCanvasEnabled = true;
            }
            else
            {
                NavigationToolBarControlModel.NavigationControlModel.GoToCanvasEnabled = false;
            }
        }

        private async void CollectionsControlViewModel_OnGoToHomePageRequestedEvent(object sender, GoToHomePageRequestedEventArgs e)
        {
            await OpenPage(DisplayPageType.HomePage);
        }

        private void CollectionsControlViewModel_OnOpenNewCanvasRequestedEventEvent(object sender, OpenNewCanvasRequestedEventArgs e)
        {
            OpenNewCanvas();
        }

        private void CollectionsControlViewModel_OnCollectionItemsInitializationFinishedEvent(object sender, CollectionItemsInitializationFinishedEventArgs e)
        {
            if (!_currentCollectionContainer.CanvasInitializing)
            {
                // Re-enable navigation after items have loaded
                NavigationToolBarControlModel.NavigationControlModel.NavigateBackLoading = false;
                NavigationToolBarControlModel.NavigationControlModel.NavigateForwardLoading = false;
                CheckNavigation();
            }
        }

        private void CollectionsControlViewModel_OnCollectionItemsInitializationStartedEvent(object sender, CollectionItemsInitializationStartedEventArgs e)
        {
            // Set tip if necessary
            PasteCanvasPageModel?.SetTipText(e.infoText, e.tipWithDelay);

            // Show navigation loading
            NavigationToolBarControlModel.NavigationControlModel.NavigateBackLoading = true;
            if (!_currentCollectionContainer.IsOnNewCanvas)
            {
                // Also show loading for forward button if not on new canvas
                NavigationToolBarControlModel.NavigationControlModel.NavigateForwardLoading = true;
            }
            CheckNavigation();
        }

        private void CollectionsControlViewModel_OnCollectionAddedEvent(object sender, CollectionAddedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void CollectionsControlViewModel_OnCollectionRemovedEvent(object sender, CollectionRemovedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void CollectionsControlViewModel_OnCollectionSelectionChangedEvent(object sender, CollectionSelectionChangedEventArgs e)
        {
            _currentCollectionContainer = e.selectedCollection;

            if (_currentCollectionContainer.CanOpenCollection)
            {
                NavigationToolBarControlModel.NavigationControlModel.GoToCanvasEnabled = true;
            }
            else
            {
                NavigationToolBarControlModel.NavigationControlModel.GoToCanvasEnabled = false;
            }
        }

        private async void CollectionsControlViewModel_OnCollectionOpenRequestedEvent(object sender, CollectionOpenRequestedEventArgs e)
        {
            await OpenPage(DisplayPageType.CanvasPage);


            // TODO: In the future, open collection preview
            return;
            //await OpenPage(DisplayPageType.CollectionsPreview); // This operation is not implemented
        }

        #endregion

        #region PasteCanvasModel

        private void PasteCanvasModel_OnProgressReportedEvent(object sender, ProgressReportedEventArgs e)
        {
            // TODO: Implement this
        }

        private void PasteCanvasModel_OnErrorOccurredEvent(object sender, ErrorOccurredEventArgs e)
        {
            // TODO: Implement this
        }

        private void PasteCanvasModel_OnFileDeletedEvent(object sender, FileDeletedEventArgs e)
        {
            // TODO: Implement this
        }

        private void PasteCanvasModel_OnFileModifiedEvent(object sender, FileModifiedEventArgs e)
        {
            // TODO: Implement this
        }

        private void PasteCanvasModel_OnFileCreatedEvent(object sender, FileCreatedEventArgs e)
        {
            _currentCollectionContainer.RefreshAddItem(e.file, e.contentType);
        }

        private async void PasteCanvasModel_OnPasteRequestedEvent(object sender, PasteRequestedEventArgs e)
        {
            if (e.isFilled)
            {
                // Already has content, meaning we need to switch to the next page
                OpenNewCanvas();

                // Forward the paste operation
                await PasteCanvasPageModel.PasteCanvasModel.TryPasteData(e.forwardedDataPackage, _canvasLoadCancellationTokenSource.Token);
            }
        }

        private async void PasteCanvasModel_OnContentLoadedEvent(object sender, ContentLoadedEventArgs e)
        {
            CheckNavigation();

            await SetSuggestedActions();
        }

        private void PasteCanvasModel_OnOpenNewCanvasRequestedEvent(object sender, OpenNewCanvasRequestedEventArgs e)
        {
            OpenNewCanvas();
        }

        #endregion

        #endregion

        #region Public Helpers

        public async Task InitializeAfterLoad()
        {
            HookTitleBarEvents();
            HookToolbarEvents();

            NavigationToolBarControlModel.NavigationControlModel.NavigateBackEnabled = false;
            NavigationToolBarControlModel.NavigationControlModel.NavigateForwardEnabled = false;

            await CollectionsControlViewModel.ReloadAllCollections();
            await OpenPage(DisplayPageType.CanvasPage);

            // We must hook them here because only now it is not null
            // Hook events if the page navigated is canvas page
            if (CurrentPageNavigation != null && CurrentPage == DisplayPageType.CanvasPage)
            {
                HookCanvasControlEvents();
            }

            NavigationToolBarControlModel.NotifyCurrentPageChanged(CurrentPageNavigation);

            UpdateTitleBar();
            CheckNavigation();
            await SetSuggestedActions();
        }

        #endregion

        #region Private Helpers

        private void UpdateViewForCompactOverlayMode()
        {
            Debugger.Break(); // TODO: Improve Compact Overlay mode
        }

        private void UpdateViewForDefaultMode()
        {

        }

        private async Task<bool> OpenPage(DisplayPageType pageType, bool simulateNavigation = false)
        {
            if (pageType == DisplayPageType.CanvasPage)
            {
                _currentCollectionContainer.CheckCanOpenCollection();
                if (_currentCollectionContainer == null || !_currentCollectionContainer.CanOpenCollection)
                {
                    // Something went wrong, cannot open CanvasPage
                    // TODO: Inform the user about the error
                    if (CurrentPageNavigation == null)
                    {
                        // Avoid unexpected exceptions
                        CurrentPageNavigation = new DisplayFrameNavigationDataModel(DisplayPageType.HomePage, _currentCollectionContainer);
                    }

                    return false;
                }
            }

            NavigationTransitionInfo navigationTransition = new DrillInNavigationTransitionInfo();

            // Navigate
            CurrentPageNavigation = new DisplayFrameNavigationDataModel(pageType, _currentCollectionContainer, navigationTransition, simulateNavigation);

            // Handle event where user opens canvas - and show loading rings if needed
            if (CurrentPage == DisplayPageType.CanvasPage && _currentCollectionContainer.CanvasInitializing)
            {
                // Show navigation loading
                NavigationToolBarControlModel.NavigationControlModel.NavigateBackLoading = true;
                if (!_currentCollectionContainer.IsOnNewCanvas)
                {
                    // Also show loading for forward button if not on new canvas
                    NavigationToolBarControlModel.NavigationControlModel.NavigateForwardLoading = true;
                }
                CheckNavigation();
            }
            // Handle event when the collection is not initialized
            else if (!_currentCollectionContainer.CanvasInitialized)
            {
                // Performance optimization: instead of initializing all collections at once,
                // initialize the one that's being opened
                await _currentCollectionContainer.InitializeItems();
            }
            // Handle event where the loading rings were shown and the collection is no longer initializing - hide them
            else
            {
                if (!_currentCollectionContainer.CanvasInitializing)
                {
                    // Re-enable navigation after items have loaded
                    NavigationToolBarControlModel.NavigationControlModel.NavigateBackLoading = false;
                    NavigationToolBarControlModel.NavigationControlModel.NavigateForwardLoading = false;
                    CheckNavigation();
                }
            }

            switch (CurrentPage)
            {
                case DisplayPageType.CanvasPage:
                    {
                        if (!_currentCollectionContainer.CanvasInitializing)
                        {
                            // We might navigate from home to a canvas that's already filled, so initialize the content
                            if (_currentCollectionContainer.IsFilled)
                            {
                                await _currentCollectionContainer.LoadCanvasFromCollection(PasteCanvasPageModel.PasteCanvasModel, _canvasLoadCancellationTokenSource.Token);
                            }
                        }
                        break;
                    }
            }

            return true;
        }

        private void CheckNavigation()
        {
            if (_currentCollectionContainer != null)
            {
                if (NavigationToolBarControlModel.NavigationControlModel.NavigateBackLoading)
                {
                    NavigationToolBarControlModel.NavigationControlModel.NavigateBackEnabled = false;
                }
                else
                {
                    NavigationToolBarControlModel.NavigationControlModel.NavigateBackEnabled = _currentCollectionContainer.HasBack();
                }

                if (NavigationToolBarControlModel.NavigationControlModel.NavigateForwardLoading)
                {
                    NavigationToolBarControlModel.NavigationControlModel.NavigateForwardEnabled = false;
                }
                else
                {
                    NavigationToolBarControlModel.NavigationControlModel.NavigateForwardEnabled = _currentCollectionContainer.HasNext();
                }
            }
        }

        private void OpenNewCanvas()
        {
            _currentCollectionContainer.DangerousSetIndex(_currentCollectionContainer.Items.Count);

            CurrentPageNavigation = new DisplayFrameNavigationDataModel(
                    CurrentPageNavigation.pageType,
                    CurrentPageNavigation.collectionContainer,
                    CurrentPageNavigation.transitionInfo, true);
        }

        private async void OnPageNavigated(DisplayFrameNavigationDataModel navigationDataModel)
        {
            UpdateTitleBar();

            await SetSuggestedActions();
        }

        private async Task SetSuggestedActions()
        {
            if (NavigationToolBarControlModel == null)
            {
                return;
            }

            switch (CurrentPage)
            {
                case DisplayPageType.CanvasPage:
                    {
                        // Add suggested actions
                        if (_currentCollectionContainer.IsFilled && PasteCanvasPageModel != null)
                        {
                            NavigationToolBarControlModel.SuggestedActionsControlModel.SetActions(await PasteCanvasPageModel.PasteCanvasModel.GetSuggestedActions());
                        }
                        else
                        {
                            NavigationToolBarControlModel.SuggestedActionsControlModel.SetActions(SuggestedActionsHelpers.GetActionsForEmptyCanvasPage(PasteCanvasPageModel.PasteCanvasModel));
                        }

                        break;
                    }

                case DisplayPageType.HomePage:
                    {
                        NavigationToolBarControlModel.SuggestedActionsControlModel.SetActions(SuggestedActionsHelpers.GetActionsForUnselectedCollection());

                        break;
                    }
            }
        }

        private void UpdateTitleBar()
        {
            if (WindowTitleBarControlModel == null)
            {
                return;
            }

            switch(CurrentPage)
            {
                case DisplayPageType.HomePage:
                    {
                        WindowTitleBarControlModel.SetTitleBarForCollectionsView();

                        break;
                    }

                case DisplayPageType.CanvasPage:
                    {
                        WindowTitleBarControlModel.SetTitleBarForCanvasView(_currentCollectionContainer.Name);

                        break;
                    }

                case DisplayPageType.CollectionsPreview:
                    {
                        WindowTitleBarControlModel.SetTitleBarForDefaultView();

                        break;
                    }
            }
        }

        #endregion

        #region Event Hooks

        private void UnhookTitleBarEvents()
        {
            if (this.WindowTitleBarControlModel != null)
            {
                this.WindowTitleBarControlModel.OnSwitchApplicationViewRequestedEvent -= WindowTitleBarControlModel_OnSwitchApplicationViewRequestedEvent;
            }
        }

        private void HookTitleBarEvents()
        {
            UnhookTitleBarEvents();
            if (this.WindowTitleBarControlModel != null)
            {
                this.WindowTitleBarControlModel.OnSwitchApplicationViewRequestedEvent += WindowTitleBarControlModel_OnSwitchApplicationViewRequestedEvent;
            }
        }

        private void HookToolbarEvents()
        {
            UnhookToolbarEvents();
            if (this.NavigationToolBarControlModel != null && this.NavigationToolBarControlModel.NavigationControlModel != null)
            {
                this.NavigationToolBarControlModel.NavigationControlModel.OnNavigateLastRequestedEvent += NavigationControlModel_OnNavigateLastRequestedEvent;
                this.NavigationToolBarControlModel.NavigationControlModel.OnNavigateBackRequestedEvent += NavigationControlModel_OnNavigateBackRequestedEvent;
                this.NavigationToolBarControlModel.NavigationControlModel.OnNavigateFirstRequestedEvent += NavigationControlModel_OnNavigateFirstRequestedEvent;
                this.NavigationToolBarControlModel.NavigationControlModel.OnNavigateForwardRequestedEvent += NavigationControlModel_OnNavigateForwardRequestedEvent;
                this.NavigationToolBarControlModel.NavigationControlModel.OnGoToHomePageRequestedEvent += NavigationControlModel_OnGoToHomePageRequestedEvent;
                this.NavigationToolBarControlModel.NavigationControlModel.OnGoToCanvasRequestedEvent += NavigationControlModel_OnGoToCanvasRequestedEvent;
            }
        }

        private void UnhookToolbarEvents()
        {
            if (this.NavigationToolBarControlModel != null && this.NavigationToolBarControlModel.NavigationControlModel != null)
            {
                this.NavigationToolBarControlModel.NavigationControlModel.OnNavigateLastRequestedEvent -= NavigationControlModel_OnNavigateLastRequestedEvent;
                this.NavigationToolBarControlModel.NavigationControlModel.OnNavigateBackRequestedEvent -= NavigationControlModel_OnNavigateBackRequestedEvent;
                this.NavigationToolBarControlModel.NavigationControlModel.OnNavigateForwardRequestedEvent -= NavigationControlModel_OnNavigateForwardRequestedEvent;
                this.NavigationToolBarControlModel.NavigationControlModel.OnNavigateFirstRequestedEvent -= NavigationControlModel_OnNavigateFirstRequestedEvent;
                this.NavigationToolBarControlModel.NavigationControlModel.OnGoToHomePageRequestedEvent -= NavigationControlModel_OnGoToHomePageRequestedEvent;
                this.NavigationToolBarControlModel.NavigationControlModel.OnGoToCanvasRequestedEvent -= NavigationControlModel_OnGoToCanvasRequestedEvent;
            }
        }

        private void HookCollectionsEvents()
        {
            UnhookCollectionsEvents();
            CollectionsControlViewModel.OnCollectionOpenRequestedEvent += CollectionsControlViewModel_OnCollectionOpenRequestedEvent;
            CollectionsControlViewModel.OnCollectionSelectionChangedEvent += CollectionsControlViewModel_OnCollectionSelectionChangedEvent;
            CollectionsControlViewModel.OnCollectionRemovedEvent += CollectionsControlViewModel_OnCollectionRemovedEvent;
            CollectionsControlViewModel.OnCollectionAddedEvent += CollectionsControlViewModel_OnCollectionAddedEvent;
            CollectionsControlViewModel.OnCollectionItemsInitializationStartedEvent += CollectionsControlViewModel_OnCollectionItemsInitializationStartedEvent;
            CollectionsControlViewModel.OnCollectionItemsInitializationFinishedEvent += CollectionsControlViewModel_OnCollectionItemsInitializationFinishedEvent;
            CollectionsControlViewModel.OnOpenNewCanvasRequestedEvent += CollectionsControlViewModel_OnOpenNewCanvasRequestedEventEvent;
            CollectionsControlViewModel.OnGoToHomePageRequestedEvent += CollectionsControlViewModel_OnGoToHomePageRequestedEvent;
            CollectionsControlViewModel.OnCollectionErrorRaisedEvent += CollectionsControlViewModel_OnCollectionErrorRaisedEvent;
        }

        private void UnhookCollectionsEvents()
        {
            CollectionsControlViewModel.OnCollectionOpenRequestedEvent -= CollectionsControlViewModel_OnCollectionOpenRequestedEvent;
            CollectionsControlViewModel.OnCollectionSelectionChangedEvent -= CollectionsControlViewModel_OnCollectionSelectionChangedEvent;
            CollectionsControlViewModel.OnCollectionRemovedEvent -= CollectionsControlViewModel_OnCollectionRemovedEvent;
            CollectionsControlViewModel.OnCollectionAddedEvent -= CollectionsControlViewModel_OnCollectionAddedEvent;
            CollectionsControlViewModel.OnCollectionItemsInitializationStartedEvent -= CollectionsControlViewModel_OnCollectionItemsInitializationStartedEvent;
            CollectionsControlViewModel.OnCollectionItemsInitializationFinishedEvent -= CollectionsControlViewModel_OnCollectionItemsInitializationFinishedEvent;
            CollectionsControlViewModel.OnOpenNewCanvasRequestedEvent -= CollectionsControlViewModel_OnOpenNewCanvasRequestedEventEvent;
            CollectionsControlViewModel.OnGoToHomePageRequestedEvent -= CollectionsControlViewModel_OnGoToHomePageRequestedEvent;
            CollectionsControlViewModel.OnCollectionErrorRaisedEvent -= CollectionsControlViewModel_OnCollectionErrorRaisedEvent;
        }

        private void HookCanvasControlEvents()
        {
            UnhookCanvasControlEvents();
            if (PasteCanvasPageModel != null)
            {
                this.PasteCanvasPageModel.PasteCanvasModel.OnOpenNewCanvasRequestedEvent += PasteCanvasModel_OnOpenNewCanvasRequestedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnContentLoadedEvent += PasteCanvasModel_OnContentLoadedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnPasteRequestedEvent += PasteCanvasModel_OnPasteRequestedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnFileCreatedEvent += PasteCanvasModel_OnFileCreatedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnFileModifiedEvent += PasteCanvasModel_OnFileModifiedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnFileDeletedEvent += PasteCanvasModel_OnFileDeletedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnErrorOccurredEvent += PasteCanvasModel_OnErrorOccurredEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnProgressReportedEvent += PasteCanvasModel_OnProgressReportedEvent;
            }
        }

        private void UnhookCanvasControlEvents()
        {
            if (PasteCanvasPageModel != null)
            {
                this.PasteCanvasPageModel.PasteCanvasModel.OnOpenNewCanvasRequestedEvent -= PasteCanvasModel_OnOpenNewCanvasRequestedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnContentLoadedEvent -= PasteCanvasModel_OnContentLoadedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnPasteRequestedEvent -= PasteCanvasModel_OnPasteRequestedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnFileCreatedEvent -= PasteCanvasModel_OnFileCreatedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnFileModifiedEvent -= PasteCanvasModel_OnFileModifiedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnFileDeletedEvent -= PasteCanvasModel_OnFileDeletedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnErrorOccurredEvent -= PasteCanvasModel_OnErrorOccurredEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnProgressReportedEvent -= PasteCanvasModel_OnProgressReportedEvent;
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            UnhookTitleBarEvents();
            UnhookToolbarEvents();
            UnhookCollectionsEvents();
            UnhookCanvasControlEvents();
        }

        #endregion
    }
}
