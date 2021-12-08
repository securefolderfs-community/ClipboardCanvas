using Microsoft.UI.Xaml;

namespace ClipboardCanvas.ModelViews
{
    public interface IDialogView
    {
        XamlRoot XamlRoot { get; set; }

        void Hide();
    }
}
