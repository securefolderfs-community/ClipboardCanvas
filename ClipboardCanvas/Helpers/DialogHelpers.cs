using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using System.Linq;

using ClipboardCanvas.Enums;

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

        public static bool IsAnyContentDialogOpen()
        {
            var openedDialogs = VisualTreeHelper.GetOpenPopups(Window.Current);
            return openedDialogs.Any((item) => item.Child is ContentDialog);
        }

        public static void CloseCurrentDialog()
        {
            var openedDialogs = VisualTreeHelper.GetOpenPopups(Window.Current);

            foreach (var item in openedDialogs)
            {
                if (item.Child is ContentDialog dialog)
                {
                    dialog.Hide();
                }
            }
        }
    }
}
