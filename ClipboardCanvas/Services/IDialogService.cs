using System.Threading.Tasks;
using Windows.Storage;

using ClipboardCanvas.Enums;
using ClipboardCanvas.ViewModels.Dialogs;
using ClipboardCanvas.ViewModels.UserControls.InAppNotifications;

namespace ClipboardCanvas.Services
{
    public interface IDialogService
    {
        IDialog<TViewModel> GetDialog<TViewModel>(TViewModel viewModel);

        Task<DialogResult> ShowDialog<TViewModel>(TViewModel viewModel);

        IInAppNotification GetNotification(InAppNotificationControlViewModel viewModel = null);

        Task<IInAppNotification> ShowNotification(InAppNotificationControlViewModel viewModel = null, int milliseconds = 0);

        // TODO: Add FolderPickerSettings
        Task<StorageFolder> PickSingleFolder();
    }
}
