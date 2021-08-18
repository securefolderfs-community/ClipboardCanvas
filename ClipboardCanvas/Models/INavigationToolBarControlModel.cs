using ClipboardCanvas.Enums;

namespace ClipboardCanvas.Models
{
    public interface INavigationToolBarControlModel
    {
        INavigationControlModel NavigationControlModel { get; }

        ISuggestedActionsControlModel SuggestedActionsControlModel { get; }

        bool IsStatusCenterButtonVisible { get; set; }

        void NotifyCurrentPageChanged(DisplayPageType pageType);
    }
}
