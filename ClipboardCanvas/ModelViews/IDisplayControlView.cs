using ClipboardCanvas.Models;

namespace ClipboardCanvas.ModelViews
{
    public interface IDisplayControlView
    {
        bool IntroductionPanelLoad { get; set; }

        IWindowTitleBarControlModel WindowTitleBarControlModel { get; }

        INavigationToolBarControlModel NavigationToolBarControlModel { get; }

        IIntroductionScreenPanelModel IntroductionScreenPanelModel { get; }

        IPasteCanvasPageModel PasteCanvasPageModel { get; }

        ICollectionPreviewPageModel CollectionPreviewPageModel { get; }
    }
}
