using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.ModelViews
{
    public interface IDialogView
    {
        XamlRoot XamlRoot { get; set; }
    }
}
