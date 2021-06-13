using ClipboardCanvas.Enums;
using System.Threading.Tasks;

namespace ClipboardCanvas.ViewModels.Dialogs
{
    public interface IDialog<TViewModel>
    {
        TViewModel ViewModel { get; set; }

        Task<DialogResult> ShowAsync();
    }
}
