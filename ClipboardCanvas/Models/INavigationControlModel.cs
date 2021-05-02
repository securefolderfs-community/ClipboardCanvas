using System;

namespace ClipboardCanvas.Models
{
    public interface INavigationControlModel : IInstanceNotifyModel
    {
        event EventHandler NavigateLastEvent;

        event EventHandler NavigateBackEvent;

        event EventHandler NavigateFirstEvent;

        event EventHandler NavigateForwardEvent;

        event EventHandler GoToHomeEvent;

        event EventHandler GoToCanvasEvent;

        bool NavigateBackEnabled { get; set; }

        bool NavigateForwardEnabled { get; set; }
    }
}
