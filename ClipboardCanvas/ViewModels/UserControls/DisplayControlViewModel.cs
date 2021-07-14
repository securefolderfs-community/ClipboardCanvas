using System;
using System.Threading.Tasks;
using System.Threading;
using Windows.UI.ViewManagement;
using Windows.Storage;
using Windows.UI.Xaml.Media.Animation;
using System.Diagnostics;
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
using ClipboardCanvas.EventArguments.CollectionPreview;

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
        /// Stores Canvas Preview control
        /// <br/><br/>
        /// Note: This might be null depending on the current view
        /// </summary>
        private IPasteCanvasPageModel PasteCanvasPageModel => _view?.PasteCanvasPageModel;

        /// <summary>
        /// Stores Collection Preview model
        /// <br/><br/>
        /// Note: This might be null depending on the current view
        /// </summary>
        private ICollectionPreviewPageModel CollectionPreviewPageModel => _view?.CollectionPreviewPageModel;

        private DisplayPageType CurrentPage => CurrentPageNavigation.pageType;

        private DisplayPageType _lastPage = DisplayPageType.CanvasPage;

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
                    // Set last page type
                    _lastPage = _CurrentPageNavigation.pageType;

                    // Unhook events
                    UnhookCanvasControlEvents();
                    UnhookCollectionPreviewEvents();

                    PasteCanvasPageModel?.Dispose(); // Dispose stuff
                }

                if (SetProperty(ref _CurrentPageNavigation, value))
                {
                    _CurrentPageNavigation.simulateNavigation = false;
                    NavigationToolBarControlModel?.NotifyCurrentPageChanged(CurrentPage);

                    if (_CurrentPageNavigation != null)
                    {
                        // Hook events for requested pages
                        if (CurrentPage == DisplayPageType.CanvasPage)
                        {
                            HookCanvasControlEvents();
                        }
                        else if (CurrentPage == DisplayPageType.CollectionPreviewPage)
                        {
                            HookCollectionPreviewEvents();
                        }
                    }

                    OnPageNavigated();
                }
            }
        }

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

        private async void NavigationControlModel_OnGoToHomepageRequestedEvent(object sender, EventArgs e)
        {
            await OpenPage(DisplayPageType.HomePage);
        }

        private async void NavigationControlModel_OnGoToCanvasRequestedEvent(object sender, EventArgs e)
        {
            await OpenPage(DisplayPageType.CanvasPage);
        }

        private async void NavigationControlModel_OnCollectionPreviewNavigateBackRequestedEvent(object sender, EventArgs e)
        {
            // Go back to page before Collection Preview page
            await OpenPage(_lastPage);
        }

        private async void NavigationControlModel_OnCollectionPreviewGoToCanvasRequestedEvent(object sender, EventArgs e)
        {
            if (CollectionPreviewPageModel.SelectedItem == null)
            {
                // No item is selected, open new canvas
                OpenNewCanvas();
            }
            else
            {
                // An item is selected, open that canvas
                await OpenPage(DisplayPageType.CanvasPage, false, null, CollectionPreviewPageModel.SelectedItem.CollectionItemModel);
            }
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
            NavigationToolBarControlModel.NavigationControlModel.CollectionPreviewGoToCanvasLoading = false;
        }

        private void CollectionsControlViewModel_OnCollectionItemsInitializationStartedEvent(object sender, CollectionItemsInitializationStartedEventArgs e)
        {
            // Show navigation loading
            NavigationToolBarControlModel.NavigationControlModel.NavigateBackLoading = true;

            if (CollectionPreviewPageModel?.SelectedItem != null)
            {
                NavigationToolBarControlModel.NavigationControlModel.CollectionPreviewGoToCanvasLoading = true;
            }

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
            await OpenPage(DisplayPageType.CollectionPreviewPage);
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
            _currentCollectionModel.AddCollectionItem(new CollectionItemViewModel(e.item, e.contentType));
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

        private void PasteCanvasModel_OnContentStartedLoadingEvent(object sender, ContentStartedLoadingEventArgs e)
        {
            NavigationToolBarControlModel.SuggestedActionsControlModel.ShowNoActionsLabelSuppressed = true;
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
            NavigationToolBarControlModel.SuggestedActionsControlModel.ShowNoActionsLabelSuppressed = false;
        }

        private void PasteCanvasModel_OnOpenNewCanvasRequestedEvent(object sender, OpenNewCanvasRequestedEventArgs e)
        {
            OpenNewCanvas();
        }

        #endregion

        #region CollectionPreviewPageViewModel

        private void CollectionPreviewPageModel_OnOpenNewCanvasRequestedEvent(object sender, OpenNewCanvasRequestedEventArgs e)
        {
            OpenNewCanvas(e.canvasType);
        }

        private async void CollectionPreviewPageModel_OnCanvasPreviewOpenRequestedEvent(object sender, CanvasPreviewOpenRequestedEventArgs e)
        {
            await OpenCanvasPageFromCollectionItem(e.collectionPreviewItemViewModel.CollectionItemModel, new SuppressNavigationTransitionInfo());
        }

        private void CollectionPreviewPageModel_OnCanvasPreviewSelectedItemChangedEvent(object sender, CanvasPreviewSelectedItemChangedEventArgs e)
        {
            // We must update the canvas button loading when selection is changed
            if (_currentCollectionModel.IsCollectionInitializing && e.selectedItem != null)
            {
                NavigationToolBarControlModel.NavigationControlModel.CollectionPreviewGoToCanvasLoading = true;
            }
            else
            {
                NavigationToolBarControlModel.NavigationControlModel.CollectionPreviewGoToCanvasLoading = false;
            }

            _currentCollectionModel.UpdateIndex(e.selectedItem.CollectionItemModel);
            CheckCollectionPreviewPageNavigation();
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

        // TODO: Refactor this - split into separate functions: OpenCanvasPage, OpenHomePage, OpenCollectionPreviewPage
        private bool OpenHomepage(NavigationTransitionInfo navigationTransition = null)
        {
            if (navigationTransition == null)
            {
                navigationTransition = new DrillInNavigationTransitionInfo();
            }

            // Navigate
            CurrentPageNavigation = new DisplayFrameNavigationDataModel(DisplayPageType.HomePage, new DisplayFrameNavigationParameterDataModel(_currentCollectionModel, CanvasHelpers.GetDefaultCanvasType()), navigationTransition, false);

            return true;
        }

        private async Task<bool> OpenCanvasPage(NavigationTransitionInfo navigationTransition = null, bool checkOpenedCanvas = true)
        {
            if (navigationTransition == null)
            {
                navigationTransition = new DrillInNavigationTransitionInfo();
            }

            if (!CheckCollectionAvailabilityBeforePageNavigation())
            {
                return false;
            }

            // Navigate
            CurrentPageNavigation = new DisplayFrameNavigationDataModel(DisplayPageType.CanvasPage, new DisplayFrameNavigationParameterDataModel(_currentCollectionModel, CanvasHelpers.GetDefaultCanvasType()), navigationTransition, false);

            await CheckCollectionStateOnPageNavigation();

            if (checkOpenedCanvas)
            {
                // We might navigate from home to a canvas that's already filled, so initialize the content
                if (!_currentCollectionModel.IsCollectionInitializing && !_currentCollectionModel.IsOnNewCanvas)
                {
                    _canvasLoadCancellationTokenSource.Cancel();
                    _canvasLoadCancellationTokenSource.Dispose();
                    _canvasLoadCancellationTokenSource = new CancellationTokenSource();

                    await _currentCollectionModel.LoadCanvasFromCollection(PasteCanvasPageModel.PasteCanvasModel, _canvasLoadCancellationTokenSource.Token);
                }
            }

            return true;
        }

        private async Task<bool> OpenCanvasPageFromCollectionItem(ICollectionItemModel collectionItemModel, NavigationTransitionInfo navigationTransition = null)
        {
            bool result = await OpenCanvasPage(navigationTransition, false);

            if (result && !_currentCollectionModel.IsCollectionInitializing)
            {
                _canvasLoadCancellationTokenSource.Cancel();
                _canvasLoadCancellationTokenSource.Dispose();
                _canvasLoadCancellationTokenSource = new CancellationTokenSource();

                await _currentCollectionModel.LoadCanvasFromCollection(PasteCanvasPageModel.PasteCanvasModel, _canvasLoadCancellationTokenSource.Token, collectionItemModel);
            }

            return result;
        }

        private async Task<bool> OpenCollectionPreviewPage(NavigationTransitionInfo navigationTransition = null)
        {
            if (navigationTransition == null)
            {
                navigationTransition = new EntranceNavigationTransitionInfo();
            }

            if (!CheckCollectionAvailabilityBeforePageNavigation())
            {
                return false;
            }

            // Navigate
            CurrentPageNavigation = new DisplayFrameNavigationDataModel(DisplayPageType.CollectionPreviewPage, new DisplayFrameNavigationParameterDataModel(_currentCollectionModel, CanvasHelpers.GetDefaultCanvasType()), navigationTransition, false);

            await CheckCollectionStateOnPageNavigation();

            return true;
        }

        private bool CheckCollectionAvailabilityBeforePageNavigation()
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
                        new DisplayFrameNavigationParameterDataModel(_currentCollectionModel, CanvasHelpers.GetDefaultCanvasType()));
                }

                return false;
            }

            return true;
        }

        private async Task CheckCollectionStateOnPageNavigation()
        {
            // Handle event where user opens canvas - and show loading rings if needed
            if (_currentCollectionModel.IsCollectionInitializing)
            {
                // Show navigation loading
                NavigationToolBarControlModel.NavigationControlModel.NavigateBackLoading = true;
                if (!_currentCollectionModel.IsOnNewCanvas)
                {
                    // Also show loading for forward button if not on new canvas
                    NavigationToolBarControlModel.NavigationControlModel.NavigateForwardLoading = true;
                }

                // If in Collection Preview and selected item is not null, meaning a canvas is selected
                if (CollectionPreviewPageModel?.SelectedItem != null)
                {
                    NavigationToolBarControlModel.NavigationControlModel.CollectionPreviewGoToCanvasLoading = true;
                    CheckCollectionPreviewPageNavigation();
                }

                CheckCanvasPageNavigation();
            }
            // Handle event when the collection is not initialized
            else if (!_currentCollectionModel.IsCollectionInitialized)
            {
                // Performance optimization: instead of initializing all collections at once,
                // initialize the one that's being opened
                await _currentCollectionModel.InitializeCollectionItems();
                CheckCollectionPreviewPageNavigation();
                CheckCanvasPageNavigation();
            }
            // Handle event where the loading rings were shown and the collection is no longer initializing - hide them
            else
            {
                if (!_currentCollectionModel.IsCollectionInitializing)
                {
                    // Re-enable navigation after items have loaded
                    NavigationToolBarControlModel.NavigationControlModel.NavigateBackLoading = false;
                    NavigationToolBarControlModel.NavigationControlModel.NavigateForwardLoading = false;

                    NavigationToolBarControlModel.NavigationControlModel.CollectionPreviewGoToCanvasLoading = false;
                    CheckCanvasPageNavigation();
                    CheckCollectionPreviewPageNavigation();
                }
            }
        }

        private async Task<bool> OpenPage(DisplayPageType pageType, bool simulateNavigation = false, NavigationTransitionInfo navigationTransition = null, ICollectionItemModel collectionItemModel = null)
        {
            switch (pageType)
            {
                case DisplayPageType.HomePage:
                    {
                        return OpenHomepage(navigationTransition);
                    }
            

                case DisplayPageType.CanvasPage:
                    {
                        return await OpenCanvasPage(navigationTransition);
                    }

                case DisplayPageType.CollectionPreviewPage:
                    {
                        return await OpenCollectionPreviewPage(navigationTransition);
                    }

                default:
                    return false;
            }
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

        private void CheckCollectionPreviewPageNavigation()
        {
            if (CurrentPage != DisplayPageType.CollectionPreviewPage)
            {
                return;
            }

            if (NavigationToolBarControlModel.NavigationControlModel.CollectionPreviewGoToCanvasLoading)
            {
                NavigationToolBarControlModel.NavigationControlModel.CollectionPreviewGoToCanvasEnabled = false;
            }
            else
            {
                NavigationToolBarControlModel.NavigationControlModel.CollectionPreviewGoToCanvasEnabled = true;
            }
        }

        private void OpenNewCanvas(CanvasType? canvasType = null)
        {
            if (canvasType == null)
            {
                canvasType = CanvasHelpers.GetDefaultCanvasType();
            }

            _currentCollectionModel.SetIndexOnNewCanvas();

            CurrentPageNavigation = new DisplayFrameNavigationDataModel(
                    DisplayPageType.CanvasPage,
                    new DisplayFrameNavigationParameterDataModel(_currentCollectionModel, (CanvasType)canvasType),
                    new DrillInNavigationTransitionInfo(),
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
                if (_currentCollectionModel.CurrentCollectionItemViewModel != null && _currentCollectionModel.CurrentCollectionItemViewModel.Item is StorageFile file && ReferenceFile.IsReferenceFile(file))
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
                        case DisplayPageType.HomePage:
                            {
                                NavigationToolBarControlModel.SuggestedActionsControlModel.SetActions(SuggestedActionsHelpers.GetActionsForHomepage());

                                checkAgain = false;

                                break;
                            }

                        case DisplayPageType.CanvasPage:
                            {
                                // Add suggested actions
                                if (PasteCanvasPageModel != null && (!_currentCollectionModel.IsOnNewCanvas || PasteCanvasPageModel.PasteCanvasModel.IsContentLoaded))
                                {
                                    var actions = await PasteCanvasPageModel.PasteCanvasModel.GetSuggestedActions();
                                    NavigationToolBarControlModel.SuggestedActionsControlModel.SetActions(actions);

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

                        case DisplayPageType.CollectionPreviewPage:
                            {
                                NavigationToolBarControlModel.SuggestedActionsControlModel.SetActions(SuggestedActionsHelpers.GetActionsForCollectionPreviewPage());

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

                case DisplayPageType.CollectionPreviewPage:
                    {
                        WindowTitleBarControlModel.SetTitleBarForCollectionPreview(_currentCollectionModel.DisplayName);

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
                this.NavigationToolBarControlModel.NavigationControlModel.OnGoToHomepageRequestedEvent += NavigationControlModel_OnGoToHomepageRequestedEvent;
                this.NavigationToolBarControlModel.NavigationControlModel.OnGoToCanvasRequestedEvent += NavigationControlModel_OnGoToCanvasRequestedEvent;
                this.NavigationToolBarControlModel.NavigationControlModel.OnCollectionPreviewNavigateBackRequestedEvent += NavigationControlModel_OnCollectionPreviewNavigateBackRequestedEvent;
                this.NavigationToolBarControlModel.NavigationControlModel.OnCollectionPreviewGoToCanvasRequestedEvent += NavigationControlModel_OnCollectionPreviewGoToCanvasRequestedEvent;
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
                this.NavigationToolBarControlModel.NavigationControlModel.OnGoToHomepageRequestedEvent -= NavigationControlModel_OnGoToHomepageRequestedEvent;
                this.NavigationToolBarControlModel.NavigationControlModel.OnGoToCanvasRequestedEvent -= NavigationControlModel_OnGoToCanvasRequestedEvent;
                this.NavigationToolBarControlModel.NavigationControlModel.OnCollectionPreviewNavigateBackRequestedEvent -= NavigationControlModel_OnCollectionPreviewNavigateBackRequestedEvent;
                this.NavigationToolBarControlModel.NavigationControlModel.OnCollectionPreviewGoToCanvasRequestedEvent -= NavigationControlModel_OnCollectionPreviewGoToCanvasRequestedEvent;
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
                this.PasteCanvasPageModel.PasteCanvasModel.OnContentStartedLoadingEvent += PasteCanvasModel_OnContentStartedLoadingEvent;
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
                this.PasteCanvasPageModel.PasteCanvasModel.OnContentStartedLoadingEvent -= PasteCanvasModel_OnContentStartedLoadingEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnPasteInitiatedEvent -= PasteCanvasModel_OnPasteInitiatedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnFileCreatedEvent -= PasteCanvasModel_OnFileCreatedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnFileModifiedEvent -= PasteCanvasModel_OnFileModifiedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnFileDeletedEvent -= PasteCanvasModel_OnFileDeletedEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnErrorOccurredEvent -= PasteCanvasModel_OnErrorOccurredEvent;
                this.PasteCanvasPageModel.PasteCanvasModel.OnProgressReportedEvent -= PasteCanvasModel_OnProgressReportedEvent;
            }
        }

        private void HookCollectionPreviewEvents()
        {
            UnhookCollectionPreviewEvents();
            if (CollectionPreviewPageModel != null)
            {
                this.CollectionPreviewPageModel.OnOpenNewCanvasRequestedEvent += CollectionPreviewPageModel_OnOpenNewCanvasRequestedEvent;
                this.CollectionPreviewPageModel.OnCanvasPreviewOpenRequestedEvent += CollectionPreviewPageModel_OnCanvasPreviewOpenRequestedEvent;
                this.CollectionPreviewPageModel.OnCanvasPreviewSelectedItemChangedEvent += CollectionPreviewPageModel_OnCanvasPreviewSelectedItemChangedEvent;
            }
        }

        private void UnhookCollectionPreviewEvents()
        {
            if (CollectionPreviewPageModel != null)
            {
                this.CollectionPreviewPageModel.OnOpenNewCanvasRequestedEvent -= CollectionPreviewPageModel_OnOpenNewCanvasRequestedEvent;
                this.CollectionPreviewPageModel.OnCanvasPreviewOpenRequestedEvent -= CollectionPreviewPageModel_OnCanvasPreviewOpenRequestedEvent;
                this.CollectionPreviewPageModel.OnCanvasPreviewSelectedItemChangedEvent -= CollectionPreviewPageModel_OnCanvasPreviewSelectedItemChangedEvent;
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
