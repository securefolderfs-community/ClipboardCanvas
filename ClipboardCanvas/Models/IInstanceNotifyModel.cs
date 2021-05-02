using ClipboardCanvas.DataModels;

namespace ClipboardCanvas.Models
{
    public interface IInstanceNotifyModel
    {
        void NotifyCurrentPageChanged(DisplayFrameNavigationDataModel navigationDataModel);
    }
}
