using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace ClipboardCanvas.ViewModels.Dialogs
{
    public interface IDialogService
    {
        IDialog<TViewModel> GetDialog<TViewModel>(TViewModel viewModel);

        Task<ContentDialogResult> ShowDialog<TViewModel>(TViewModel viewModel);

        // TODO: Add FolderPickerSettings
        Task<StorageFolder> PickSingleFolder();
    }
}
