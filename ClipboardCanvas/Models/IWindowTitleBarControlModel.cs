using System;

namespace ClipboardCanvas.Models
{
    public interface IWindowTitleBarControlModel
    {
        bool IsInRestrictedAccess { get; set; }

        event EventHandler OnSwitchApplicationViewRequestedEvent;

        void SetTitleBarForDefaultView();

        void SetTitleBarForCollectionsView();

        void SetTitleBarForCanvasView(string collectionName);
    }
}
