using ClipboardCanvas.Models;

namespace ClipboardCanvas.ModelViews
{
    public interface IDisplayControlView
    {
        IWindowTitleBarControlModel WindowTitleBarControlModel { get; }

        INavigationToolBarControlModel NavigationToolBarControlModel { get; }

        IPasteCanvasModel PasteCanvasModel { get; }
    }
}
