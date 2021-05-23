using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;

namespace ClipboardCanvas.Helpers
{
    public static class IconHelpers
    {
        public static async Task<(BitmapImage icon, string appName)> GetIconFromFileHandlingApp(string fileExtension)
        {
            IReadOnlyList<AppInfo> apps = await Launcher.FindFileHandlersAsync(fileExtension);

            // TODO: Select the app that opens the file
            AppInfo app = apps.Last();

            RandomAccessStreamReference stream = app.DisplayInfo.GetLogo(new Size(256, 256));

            BitmapImage bitmap = await ImagingHelpers.ToBitmapAsync((await stream.OpenReadAsync()).AsStreamForRead());

            return (bitmap, app.DisplayInfo.DisplayName);
        }
    }
}
