using System;
using Microsoft.UI.Xaml.Controls;

using ClipboardCanvas.DataModels.Navigation;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.Pages;
using ClipboardCanvas.DisplayFrameEventArgs;
using ClipboardCanvas.ViewModels.UserControls.Collections;

namespace ClipboardCanvas.Services.Implementation
{
    public class NavigationService : INavigationService
    {
        public Frame DisplayFrame { get; internal set; }

        public Func<bool> CheckCollectionAvailabilityBeforePageNavigation { get; internal set; }

        public DisplayPageType CurrentPage { get; private set; }

        public DisplayPageType LastPage { get; private set; } = DisplayPageType.CanvasPage;

        public event EventHandler<NavigationStartedEventArgs> OnNavigationStartedEvent;

        public event EventHandler<NavigationFinishedEventArgs> OnNavigationFinishedEvent;

        public bool OpenNewCanvas(ICollectionModel collection, NavigationTransitionType transitionType = NavigationTransitionType.DrillInTransition, CanvasType? canvasType = null)
        {
            if (canvasType == null)
            {
                canvasType = collection.AssociatedCanvasType;
            }

            OnNavigationStartedEvent?.Invoke(this, new NavigationStartedEventArgs(collection, (CanvasType)canvasType, DisplayPageType.CanvasPage));

            collection.SetIndexOnNewCanvas();
            
            BaseDisplayFrameParameterDataModel navigationParameter = new CanvasPageNavigationParameterModel(collection, (CanvasType)canvasType);
            LastPage = CurrentPage;
            CurrentPage = DisplayPageType.CanvasPage;
            DisplayFrame.Navigate(typeof(CanvasPage), navigationParameter, transitionType.ToNavigationTransition());

            OnNavigationFinishedEvent?.Invoke(this, new NavigationFinishedEventArgs(true, null, collection, (CanvasType)canvasType, DisplayPageType.CanvasPage));

            return true;
        }

        public bool OpenCanvasPage(ICollectionModel collection, CollectionItemViewModel collectionItem = null, CanvasPageNavigationParameterModel navigationParameter = null, NavigationTransitionType transitionType = NavigationTransitionType.DrillInTransition, CanvasType? canvasType = null)
        {
            if (canvasType == null)
            {
                canvasType = collection.AssociatedCanvasType;
            }

            if (!CheckCollectionAvailabilityBeforePageNavigation())
            {
                OnNavigationFinishedEvent?.Invoke(this, new NavigationFinishedEventArgs(false, null, collection, (CanvasType)canvasType, DisplayPageType.CanvasPage));
                return false;
            }

            OnNavigationStartedEvent?.Invoke(this, new NavigationStartedEventArgs(collection, (CanvasType)canvasType, DisplayPageType.CanvasPage));

            if (navigationParameter == null)
            {
                navigationParameter = new CanvasPageNavigationParameterModel(collection, (CanvasType)canvasType);
            }
            LastPage = CurrentPage;
            CurrentPage = DisplayPageType.CanvasPage;
            DisplayFrame.Navigate(typeof(CanvasPage), navigationParameter, transitionType.ToNavigationTransition());

            OnNavigationFinishedEvent?.Invoke(this, new NavigationFinishedEventArgs(true, collectionItem ?? collection.CurrentCollectionItemViewModel, collection, (CanvasType)canvasType, DisplayPageType.CanvasPage));

            return true;
        }

        public bool OpenHomepage(HomepageNavigationParameterModel navigationParameter = null, NavigationTransitionType transitionType = NavigationTransitionType.DrillInTransition)
        {
            CanvasType canvasType = navigationParameter?.canvasType ?? CanvasHelpers.GetDefaultCanvasType();

            OnNavigationStartedEvent?.Invoke(this, new NavigationStartedEventArgs(null, canvasType, DisplayPageType.Homepage));

            if (navigationParameter == null)
            {
                navigationParameter = new HomepageNavigationParameterModel(canvasType);
            }
            LastPage = CurrentPage;
            CurrentPage = DisplayPageType.Homepage;
            DisplayFrame.Navigate(typeof(HomePage), navigationParameter, transitionType.ToNavigationTransition());

            OnNavigationFinishedEvent?.Invoke(this, new NavigationFinishedEventArgs(true, null, null, canvasType, DisplayPageType.Homepage));

            return true;
        }

        public bool OpenCollectionPreviewPage(ICollectionModel collection, CollectionPreviewPageNavigationParameterModel navigationParameter = null, NavigationTransitionType transitionType = NavigationTransitionType.EntranceTransition, CanvasType? canvasType = null)
        {
            if (canvasType == null)
            {
                canvasType = collection.AssociatedCanvasType;
            }

            OnNavigationStartedEvent?.Invoke(this, new NavigationStartedEventArgs(collection, (CanvasType)canvasType, DisplayPageType.CollectionPreviewPage));

            if (!CheckCollectionAvailabilityBeforePageNavigation())
            {
                OnNavigationFinishedEvent?.Invoke(this, new NavigationFinishedEventArgs(false, null, collection, (CanvasType)canvasType, DisplayPageType.CollectionPreviewPage));
                return false;
            }

            if (navigationParameter == null)
            {
                navigationParameter = new CollectionPreviewPageNavigationParameterModel(collection, (CanvasType)canvasType);
            }
            LastPage = CurrentPage;
            CurrentPage = DisplayPageType.CollectionPreviewPage;
            DisplayFrame.Navigate(typeof(CollectionPreviewPage), navigationParameter, transitionType.ToNavigationTransition());

            OnNavigationFinishedEvent?.Invoke(this, new NavigationFinishedEventArgs(true, null, collection, (CanvasType)canvasType, DisplayPageType.CollectionPreviewPage));

            return true;
        }
    }
}
