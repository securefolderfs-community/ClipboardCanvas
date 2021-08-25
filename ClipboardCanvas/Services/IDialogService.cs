using System.Threading.Tasks;
using Windows.Storage;
using System.Collections.Generic;

using ClipboardCanvas.Enums;
using ClipboardCanvas.ViewModels.Dialogs;
using ClipboardCanvas.ViewModels.UserControls.InAppNotifications;

namespace ClipboardCanvas.Services
{
    public interface IDialogService
    {
        void CloseAllDialogs();

        IDialog<TViewModel> GetDialog<TViewModel>(TViewModel viewModel);

        Task<DialogResult> ShowDialog<TViewModel>(TViewModel viewModel);

        IInAppNotification GetNotification(InAppNotificationControlViewModel viewModel = null);

        IInAppNotification ShowNotification(InAppNotificationControlViewModel viewModel = null, int milliseconds = 0);

        // TODO: Add FolderPickerSettings
        Task<StorageFolder> PickSingleFolder();

        Task<StorageFile> PickSingleFile(IEnumerable<string> filter);
    }
}
