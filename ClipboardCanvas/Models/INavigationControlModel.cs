using System;

namespace ClipboardCanvas.Models
{
    public interface INavigationControlModel : IInstanceNotifyModel
    {
        event EventHandler OnNavigateLastRequestedEvent;

        event EventHandler OnNavigateBackRequestedEvent;

        event EventHandler OnNavigateFirstRequestedEvent;

        event EventHandler OnNavigateForwardRequestedEvent;

        event EventHandler OnGoToHomePageRequestedEvent;

        event EventHandler OnGoToCanvasRequestedEvent;

        bool NavigateBackEnabled { get; set; }

        bool NavigateForwardEnabled { get; set; }
    }
}
