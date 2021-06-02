using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace ClipboardCanvas.ViewModels.Dialogs
{
    public interface IDialog<TViewModel>
    {
        TViewModel ViewModel { get; set; }

        Task<ContentDialogResult> ShowAsync();
    }
}
