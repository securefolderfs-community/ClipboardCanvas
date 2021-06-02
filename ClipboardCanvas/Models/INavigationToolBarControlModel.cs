using ClipboardCanvas.DataModels.Navigation;
using ClipboardCanvas.Enums;

namespace ClipboardCanvas.Models
{
    public interface INavigationToolBarControlModel
    {
        INavigationControlModel NavigationControlModel { get; }

        ISuggestedActionsControlModel SuggestedActionsControlModel { get; }

        void NotifyCurrentPageChanged(DisplayPageType pageType);
    }
}
