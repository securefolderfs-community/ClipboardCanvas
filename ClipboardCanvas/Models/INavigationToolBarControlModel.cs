using ClipboardCanvas.DataModels.Navigation;

namespace ClipboardCanvas.Models
{
    public interface INavigationToolBarControlModel
    {
        INavigationControlModel NavigationControlModel { get; }

        ISuggestedActionsControlModel SuggestedActionsControlModel { get; }

        void NotifyCurrentPageChanged(DisplayFrameNavigationDataModel navigationDataModel);
    }
}
