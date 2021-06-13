using ClipboardCanvas.Enums;
using ClipboardCanvas.ViewModels.Dialogs;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.Services
{
    public interface IDialogService
    {
        IDialog<TViewModel> GetDialog<TViewModel>(TViewModel viewModel);

        Task<DialogResult> ShowDialog<TViewModel>(TViewModel viewModel);

        // TODO: Add FolderPickerSettings
        Task<StorageFolder> PickSingleFolder();
    }
}
