namespace ClipboardCanvas.Models
{
    public interface INavigationToolBarControlModel : IInstanceNotifyModel
    {
        INavigationControlModel NavigationControlModel { get; }

        IAdaptiveOptionsControlModel AdaptiveOptionsControlModel { get; }
    }
}
