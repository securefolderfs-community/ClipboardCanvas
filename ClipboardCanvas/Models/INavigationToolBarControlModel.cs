namespace ClipboardCanvas.Models
{
    public interface INavigationToolBarControlModel : IInstanceNotifyModel
    {
        INavigationControlModel NavigationControlModel { get; }

        ISuggestedActionsControlModel SuggestedActionsControlModel { get; }
    }
}
