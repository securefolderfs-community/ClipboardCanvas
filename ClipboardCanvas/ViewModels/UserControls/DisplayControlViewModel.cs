using System;
using System.Threading.Tasks;
using System.Threading;
using Windows.UI.ViewManagement;
using Windows.Storage;
using Windows.UI.Xaml.Media.Animation;
using Microsoft.Toolkit.Mvvm.ComponentModel;

using ClipboardCanvas.Enums;
using ClipboardCanvas.Models;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.EventArguments.Collections;
using ClipboardCanvas.DataModels.Navigation;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ReferenceItems;
using ClipboardCanvas.ViewModels.UserControls.Collections;

namespace ClipboardCanvas.ViewModels.UserControls
{
    public class DisplayControlViewModel : ObservableObject, IDisposable
    {
        #region Private Members

        private readonly IDisplayControlView _view;

        private ICollectionModel _currentCollectionModel;

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
                    NavigationToolBarControlModel?.NotifyCurrentPageChanged(CurrentPage);

                    // Hook events if the page navigated is canvas page
                    if (_CurrentPageNavigation != null && _CurrentPageNavigation.pageType == DisplayPageType.CanvasPage)
                    {
                        HookCanvasControlEvents();
                    }

                    OnPageNavigated();
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
            if (CurrentPage == DisplayPageType.CanvasPage && _currentCollectionModel.HasBack())
            {
                _canvasLoadCancellationTokenSource.Cancel();
                _canvasLoadCancellationTokenSource.Dispose();
                _canvasLoadCancellationTokenSource = new CancellationTokenSource();

                await _currentCollectionModel.NavigateLast(PasteCanvasPageModel.PasteCanvasModel, _canvasLoadCancellationTokenSource.Token);
            }
        }

        private async void NavigationControlModel_OnNavigateBackRequestedEvent(object sender, EventArgs e)
        {
            if (CurrentPage == DisplayPageType.CanvasPage && _currentCollectionModel.HasBack())
            {
                _canvasLoadCancellationTokenSource.Cancel();
                _canvasLoadCancellationTokenSource.Dispose();
                _canvasLoadCancellationTokenSource = new CancellationTokenSource();

                await _currentCollectionModel.NavigateBack(PasteCanvasPageModel.PasteCanvasModel, _canvasLoadCancellationTokenSource.Token);
            }
        }

        private void NavigationControlModel_OnNavigateFirstRequestedEvent(object sender, EventArgs e)
        {
            if (CurrentPage == DisplayPageType.CanvasPage && _currentCollectionModel.HasNext())
            {
                _canvasLoadCancellationTokenSource.Cancel();
                _canvasLoadCancellationTokenSource.Dispose();
                _canvasLoadCancellationTokenSource = new CancellationTokenSource();

                _currentCollectionModel.NavigateFirst(PasteCanvasPageModel.PasteCanvasModel);
            }
        }

        private async void NavigationControlModel_OnNavigateForwardRequestedEvent(object sender, EventArgs e)
        {
            if (CurrentPage == DisplayPageType.CanvasPage && _currentCollectionModel.HasNext())
            {
                _canvasLoadCancellationTokenSource.Cancel();
                _canvasLoadCancellationTokenSource.Dispose();
                _canvasLoadCancellationTokenSource = new CancellationTokenSource();

                await _currentCollectionModel.NavigateNext(PasteCanvasPageModel.PasteCanvasModel, _canvasLoadCancellationTokenSource.Token);
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

        private void CollectionsControlViewModel_OnTipTextUpdateRequestedEvent(object sender, TipTextUpdateRequestedEventArgs e)
        {
            PasteCanvasPageModel.SetTipText(e.infoText, e.tipShowDelay);
        }

        private async void CollectionsControlViewModel_OnCanvasLoadFailedEvent(object sender, CanvasLoadFailedEventArgs e)
        {
            CheckCanvasPageNavigation();

            await SetSuggestedActions(e.error);
        }

        private void CollectionsControlViewModel_OnCollectionErrorRaisedEvent(object sender, CollectionErrorRaisedEventArgs e)
        {
            if (e.safeWrapperResult)
            {
                NavigationToolBarControlModel.NavigationControlModel.GoToCanvasEnabled = true;
            }
            else
            {
                NavigationToolBarControlModel.NavigationControlModel.GoToCanvasEnabled = false;
            }
        }

        private async void CollectionsControlViewModel_OnGoToHomepageRequestedEvent(object sender, GoToHomepageRequestedEventArgs e)
        {
            await OpenPage(DisplayPageType.HomePage);
        }

        private void CollectionsControlViewModel_OnOpenNewCanvasRequestedEvent(object sender, OpenNewCanvasRequestedEventArgs e)
        {
            OpenNewCanvas();
        }

        private void CollectionsControlViewModel_OnCollectionItemsInitializationFinishedEvent(object sender, CollectionItemsInitializationFinishedEventArgs e)
        {
            // Re-enable navigation after items have loaded
            NavigationToolBarControlModel.NavigationControlModel.NavigateBackLoading = false;
            NavigationToolBarControlModel.NavigationControlModel.NavigateForwardLoading = false;
        }

        private void CollectionsControlViewModel_OnCollectionItemsInitializationStartedEvent(object sender, CollectionItemsInitializationStartedEventArgs e)
        {
            // Show navigation loading
            NavigationToolBarControlModel.NavigationControlModel.NavigateBackLoading = true;
            if (!_currentCollectionModel.IsOnNewCanvas)
            {
                // Also show loading for forward button if not on new canvas
                NavigationToolBarControlModel.NavigationControlModel.NavigateForwardLoading = true;
            }
        }

        private void CollectionsControlViewModel_OnCollectionAddedEvent(object sender, CollectionAddedEventArgs e)
        {
        }

        private void CollectionsControlViewModel_OnCollectionRemovedEvent(object sender, CollectionRemovedEventArgs e)
        {
        }

        private void CollectionsControlViewModel_OnCollectionSelectionChangedEvent(object sender, CollectionSelectionChangedEventArgs e)
        {
            _currentCollectionModel = e.baseCollectionViewModel;

            if (_currentCollectionModel.IsCollectionAvailable)
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

        private async void PasteCanvasModel_OnFileDeletedEvent(object sender, FileDeletedEventArgs e)
        {
            await _currentCollectionModel.LoadCanvasFromCollection(PasteCanvasPageModel.PasteCanvasModel, _canvasLoadCancellationTokenSource.Token);
        }

        private void PasteCanvasModel_OnFileModifiedEvent(object sender, FileModifiedEventArgs e)
        {
            // TODO: Implement this
        }

        private void PasteCanvasModel_OnFileCreatedEvent(object sender, FileCreatedEventArgs e)
        {
            _currentCollectionModel.AddCollectionItem(new CollectionItemViewModel(e.file, e.contentType));
        }

        private async void PasteCanvasModel_OnPasteInitiatedEvent(object sender, PasteInitiatedEventArgs e)
        {
            if (e.isFilled)
            {
                // Already has content, meaning we need to switch to the next page
                OpenNewCanvas();

                // Forward the paste operation
                SafeWrapperResult result = await PasteCanvasPageModel.PasteCanvasModel.TryPasteData(e.forwardedDataPackage, _canvasLoadCancellationTokenSource.Token);
            }
        }

        private async void PasteCanvasModel_OnContentLoadedEvent(object sender, ContentLoadedEventArgs e)
        {
            if (CurrentPage == DisplayPageType.CanvasPage
                && (e.contentDataModel is TextContentType
                || e.contentDataModel is MarkdownContentType))
            {
                WindowTitleBarControlModel.ShowTitleUnderline = true;
            }
            else
            {
                WindowTitleBarControlModel.ShowTitleUnderline = false;
            }

            CheckCanvasPageNavigation();

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

            NavigationToolBarControlModel.NotifyCurrentPageChanged(CurrentPage);

            //UpdateTitleBar();
            CheckCanvasPageNavigation();
        }

        #endregion

        #region Private Helpers

        private void UpdateViewForCompactOverlayMode()
        {
        }

        private void UpdateViewForDefaultMode()
        {
        }

        private async Task<bool> OpenPage(DisplayPageType pageType, bool simulateNavigation = false)
        {
            if (pageType == DisplayPageType.CanvasPage)
            {
                _currentCollectionModel?.CheckCollectionAvailability();
                if (_currentCollectionModel == null || !_currentCollectionModel.IsCollectionAvailable)
                {
                    // Something went wrong, cannot open CanvasPage
                    // TODO: Inform the user about the error
                    if (CurrentPageNavigation == null)
                    {
                        // Avoid unwanted exceptions
                        CurrentPageNavigation = new DisplayFrameNavigationDataModel(DisplayPageType.HomePage,
                            new DisplayFrameNavigationParameterDataModel(_currentCollectionModel));
                    }

                    return false;
                }
            }

            NavigationTransitionInfo navigationTransition = new DrillInNavigationTransitionInfo();

            // Navigate
            CurrentPageNavigation = new DisplayFrameNavigationDataModel(pageType, new DisplayFrameNavigationParameterDataModel(_currentCollectionModel), navigationTransition, simulateNavigation);

            // Handle event where user opens canvas - and show loading rings if needed
            if (CurrentPage == DisplayPageType.CanvasPage && _currentCollectionModel.IsCanvasInitializing)
            {
                // Show navigation loading
                NavigationToolBarControlModel.NavigationControlModel.NavigateBackLoading = true;
                if (!_currentCollectionModel.IsOnNewCanvas)
                {
                    // Also show loading for forward button if not on new canvas
                    NavigationToolBarControlModel.NavigationControlModel.NavigateForwardLoading = true;
                }
                CheckCanvasPageNavigation();
            }
            // Handle event when the collection is not initialized
            else if (!_currentCollectionModel.IsCanvasInitialized)
            {
                // Performance optimization: instead of initializing all collections at once,
                // initialize the one that's being opened
                await _currentCollectionModel.InitializeCollectionItems();
                CheckCanvasPageNavigation();
            }
            // Handle event where the loading rings were shown and the collection is no longer initializing - hide them
            else
            {
                if (!_currentCollectionModel.IsCanvasInitializing)
                {
                    // Re-enable navigation after items have loaded
                    NavigationToolBarControlModel.NavigationControlModel.NavigateBackLoading = false;
                    NavigationToolBarControlModel.NavigationControlModel.NavigateForwardLoading = false;
                    CheckCanvasPageNavigation();
                }
            }

            switch (CurrentPage)
            {
                case DisplayPageType.CanvasPage:
                    {
                        if (!_currentCollectionModel.IsCanvasInitializing)
                        {
                            // We might navigate from home to a canvas that's already filled, so initialize the content
                            if (!_currentCollectionModel.IsOnNewCanvas)
                            {
                                _canvasLoadCancellationTokenSource.Cancel();
                                _canvasLoadCancellationTokenSource.Dispose();
                                _canvasLoadCancellationTokenSource = new CancellationTokenSource();

                                await _currentCollectionModel.LoadCanvasFromCollection(PasteCanvasPageModel.PasteCanvasModel, _canvasLoadCancellationTokenSource.Token);
                            }
                        }
                        break;
                    }
            }

            return true;
        }

        private void CheckCanvasPageNavigation()
        {
            if (CurrentPage != DisplayPageType.CanvasPage)
            {
                return;
            }

            if (_currentCollectionModel != null)
            {
                if (NavigationToolBarControlModel.NavigationControlModel.NavigateBackLoading)
                {
                    NavigationToolBarControlModel.NavigationControlModel.NavigateBackEnabled = false;
                }
                else
                {
                    NavigationToolBarControlModel.NavigationControlModel.NavigateBackEnabled = _currentCollectionModel.HasBack();
                }

                if (NavigationToolBarControlModel.NavigationControlModel.NavigateForwardLoading)
                {
                    NavigationToolBarControlModel.NavigationControlModel.NavigateForwardEnabled = false;
                }
                else
                {
                    NavigationToolBarControlModel.NavigationControlModel.NavigateForwardEnabled = _currentCollectionModel.HasNext();
                }
            }
        }

        private void OpenNewCanvas()
        {
            _currentCollectionModel.SetIndexOnNewCanvas();

            CurrentPageNavigation = new DisplayFrameNavigationDataModel(
                    CurrentPageNavigation.pageType,
                    CurrentPageNavigation.parameter,
                    CurrentPageNavigation.transitionInfo,
                    true);
        }

        private async void OnPageNavigated()
        {
            WindowTitleBarControlModel.ShowTitleUnderline = false;

            UpdateTitleBar();
            CheckCanvasPageNavigation();
            await SetSuggestedActions();
        }

        private async Task SetSuggestedActions(SafeWrapperResult fromError = null)
        {
            if (NavigationToolBarControlModel == null)
            {
                return;
            }

            if (fromError != null)
            {
                if (ReferenceFile.IsReferenceFile(_currentCollectionModel.CurrentCollectionItemViewModel.File))
                {
                    if (fromError == OperationErrorCode.InvalidArgument) // Reference File is corrupted
                    {
                        NavigationToolBarControlModel.SuggestedActionsControlModel.SetActions(SuggestedActionsHelpers.GetActionsForInvalidReference(PasteCanvasPageModel.PasteCanvasModel));
                    }
                    else if (fromError == OperationErrorCode.NotFound) // Reference not found
                    {
                        NavigationToolBarControlModel.SuggestedActionsControlModel.SetActions(SuggestedActionsHelpers.GetActionsForInvalidReference(PasteCanvasPageModel.PasteCanvasModel));
                    }
                }
            }
            else
            {
                bool checkAgain = false;
                do
                {
                    switch (CurrentPage)
                    {
                        case DisplayPageType.CanvasPage:
                            {
                                // Add suggested actions
                                if (PasteCanvasPageModel.PasteCanvasModel.IsFilled && PasteCanvasPageModel != null)
                                {
                                    NavigationToolBarControlModel.SuggestedActionsControlModel.SetActions(await PasteCanvasPageModel.PasteCanvasModel.GetSuggestedActions());

                                    // Check again, the state might have changed
                                    if (_currentCollectionModel.IsOnNewCanvas)
                                    {
                                        // The state changed
                                        checkAgain = true;
                                        break;
                                    }

                                    checkAgain = false;
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

                                checkAgain = false;

                                break;
                            }
                    }
                }
                while (checkAgain);
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
                        WindowTitleBarControlModel.SetTitleBarForCanvasView(_currentCollectionModel.DisplayName);

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
            CollectionsControlViewModel.OnOpenNewCanvasRequestedEvent += CollectionsControlViewModel_OnOpenNewCanvasRequestedEvent;
            CollectionsControlViewModel.OnGoToHomepageRequestedEvent += CollectionsControlViewModel_OnGoToHomepageRequestedEvent;
            CollectionsControlViewModel.OnCollectionErrorRaisedEvent += CollectionsControlViewModel_OnCollectionErrorRaisedEvent;
            CollectionsControlViewModel.OnCanvasLoadFailedEvent += CollectionsControlViewModel_OnCanvasLoadFailedEvent;
            CollectionsControlViewModel.OnTipTextUpdateRequestedEvent += CollectionsControlViewModel_OnTipTextUpdateRequestedEvent;
        }

        private void UnhookCollectionsEvents()
        {
            CollectionsControlViewModel.OnCollectionOpenRequestedEvent -= CollectionsControlViewModel_OnCollectionOpenRequestedEvent;
            CollectionsControlViewModel.OnCollectionSelectionChangedEvent -= CollectionsControlViewModel_OnCollectionSelectionChangedEvent;
            CollectionsControlViewModel.OnCollectionRemovedEvent -= CollectionsControlViewModel_OnCollectionRemovedEvent;
            CollectionsControlViewModel.OnCollectionAddedEvent -= CollectionsControlViewModel_OnCollectionAddedEvent;
            CollectionsControlViewModel.OnCollectionItemsInitializationStartedEvent -= CollectionsControlViewModel_OnCollectionItemsInitializationStartedEvent;
            CollectionsControlViewModel.OnCollectionItemsInitializationFinishedEvent -= CollectionsControlViewModel_OnCollectionItemsInitializationFinishedEvent;
            CollectionsControlViewModel.OnOpenNewCanvasRequestedEvent -= CollectionsControlViewModel_OnOpenNewCanvasRequestedEvent;
            CollectionsControlViewModel.OnGoToHomepageRequestedEvent -= CollectionsControlViewModel_OnGoToHomepageRequestedEvent;
            CollectionsControlViewModel.OnCollectionErrorRaisedEvent -= CollectionsControlViewModel_OnCollectionErrorRaisedEvent;
            CollectionsControlViewModel.OnCanvasLoadFailedEvent -= CollectionsControlViewModel_OnCanvasLoadFailedEvent;
            CollectionsControlViewModel.OnTipTextUpdateRequestedEvent -= CollectionsControlViewModel_OnTipTextUpdateRequestedEvent;
        }

        private void HookCanvasControlEvents()
        {
            UnhookCanvasControlEvents();
            if (PasteCanvasPageModel != null)
            {
                this.PasteCanvasPageModel.PasteCanvasModel.OnOpenNewCanvasRequestedEvent += PasteCanvasModel_OnOpenNewCanvasRequestedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnContentLoadedEvent += PasteCanvasModel_OnContentLoadedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnPasteInitiatedEvent += PasteCanvasModel_OnPasteInitiatedEvent;
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
                this.PasteCanvasPageModel.PasteCanvasModel.OnPasteInitiatedEvent -= PasteCanvasModel_OnPasteInitiatedEvent;
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
