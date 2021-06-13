using ClipboardCanvas.Enums;
using Windows.UI.Xaml.Controls;

namespace ClipboardCanvas.Helpers
{
    public static class DialogHelpers
    {
        public static DialogResult ToDialogResult(this ContentDialogResult contentDialogResult)
        {
            switch (contentDialogResult)
            {
                case ContentDialogResult.Primary:
                    return DialogResult.Primary;

                case ContentDialogResult.Secondary:
                    return DialogResult.Secondary;

                default:
                case ContentDialogResult.None:
                    return DialogResult.Cancel;
            }
        }
    }
}
