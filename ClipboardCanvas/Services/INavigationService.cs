using System;

using ClipboardCanvas.DataModels.Navigation;
using ClipboardCanvas.DisplayFrameEventArgs;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.UserControls.Collections;

namespace ClipboardCanvas.Services
{
    public interface INavigationService
    {
        DisplayPageType CurrentPage { get; }

        DisplayPageType LastPage { get; }

        event EventHandler<NavigationStartedEventArgs> OnNavigationStartedEvent;

        event EventHandler<NavigationFinishedEventArgs> OnNavigationFinishedEvent;

        bool OpenNewCanvas(ICollectionModel collection, NavigationTransitionType transitionType = NavigationTransitionType.DrillInTransition, CanvasType? canvasType = null);

        bool OpenCanvasPage(ICollectionModel collection, CollectionItemViewModel collectionItem = null, CanvasPageNavigationParameterModel navigationParameter = null, NavigationTransitionType transitionType = NavigationTransitionType.DrillInTransition);

        bool OpenHomepage(HomepageNavigationParameterModel navigationParameter = null, NavigationTransitionType transitionType = NavigationTransitionType.DrillInTransition);

        bool OpenCollectionPreviewPage(ICollectionModel collection, CollectionPreviewPageNavigationParameterModel navigationParameter = null, NavigationTransitionType transitionType = NavigationTransitionType.EntranceTransition, CanvasType? canvasType = null);
    }
}
