using System;

namespace ClipboardCanvas.Models
{
    public interface IWindowTitleBarControlModel
    {
        event EventHandler OnSwitchApplicationViewRequestedEvent;

        bool IsInRestrictedAccess { get; set; }

        bool ShowTitleUnderline { get; set; }

        void SetTitleBarForDefaultView();

        void SetTitleBarForCollectionsView();

        void SetTitleBarForCanvasView(string collectionName);

        void SetTitleBarForCollectionPreview(string collectionName);
    }
}
