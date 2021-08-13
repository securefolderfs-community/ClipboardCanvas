using System.Threading.Tasks;

using ClipboardCanvas.Enums;

namespace ClipboardCanvas.ViewModels.Dialogs
{
    public interface IDialog<TViewModel>
    {
        TViewModel ViewModel { get; set; }

        Task<DialogResult> ShowAsync();
    }
}
