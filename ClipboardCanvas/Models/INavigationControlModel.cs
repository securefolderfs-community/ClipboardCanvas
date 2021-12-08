using System;
using ClipboardCanvas.Enums;

namespace ClipboardCanvas.Models
{
    public interface INavigationControlModel
    {
        event EventHandler OnNavigateLastRequestedEvent;

        event EventHandler OnNavigateBackRequestedEvent;

        event EventHandler OnNavigateFirstRequestedEvent;

        event EventHandler OnNavigateForwardRequestedEvent;

        event EventHandler OnGoToHomepageRequestedEvent;

        event EventHandler OnGoToCanvasRequestedEvent;

        event EventHandler OnCollectionPreviewNavigateBackRequestedEvent;

        event EventHandler OnCollectionPreviewGoToCanvasRequestedEvent;

        bool NavigateBackEnabled { get; set; }

        bool NavigateBackLoading { get; set; }

        bool NavigateForwardEnabled { get; set; }

        bool NavigateForwardLoading { get; set; }

        bool GoToCanvasEnabled { get; set; }

        bool CollectionPreviewGoToCanvasEnabled { get; set; }

        bool CollectionPreviewGoToCanvasLoading { get; set; }

        void NotifyCurrentPageChanged(DisplayPageType pageType);
    }
}
