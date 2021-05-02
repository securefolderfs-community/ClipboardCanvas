using System;

namespace ClipboardCanvas.Models
{
    public interface IWindowTitleBarControlModel
    {
        event EventHandler OnSwitchApplicationViewRequestedEvent;

        void SetTitleBarForDefaultView();

        void SetTitleBarForCollectionsView();

        void SetTitleBarForCanvasView(string collectionName);
    }
}
