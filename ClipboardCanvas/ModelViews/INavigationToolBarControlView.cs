using ClipboardCanvas.Models;

namespace ClipboardCanvas.ModelViews
{
    public interface INavigationToolBarControlView
    {
        INavigationControlModel NavigationControlModel { get; }

        ISuggestedActionsControlModel SuggestedActionsControlModel { get; }
    }
}
