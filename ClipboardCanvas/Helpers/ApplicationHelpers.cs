using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Services.Store;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Microsoft.UI.Xaml.Media.Imaging;
using ClipboardCanvas.Extensions;

namespace ClipboardCanvas.Helpers
{
    public static class ApplicationHelpers
    {
        [Obsolete]
        public static async Task<(BitmapImage icon, string appName)> GetIconFromFileHandlingApp(StorageFile file, string fileExtension)
        {
            IReadOnlyList<AppInfo> apps = await Launcher.FindFileHandlersAsync(fileExtension);

            AppInfo app = apps.FirstOrDefault();

            if (app == null)
            {
                return (null, null);
            }

            RandomAccessStreamReference stream = app.DisplayInfo.GetLogo(new Size(256, 256));

            BitmapImage bitmap = await ImagingHelpers.ToBitmapAsync(await stream.OpenReadAsync());

            return (bitmap, app.DisplayInfo.DisplayName);
        }

        public static async Task<bool> IsStoreUpdateAvailable()
        {
            try
            {
                IReadOnlyList<StorePackageUpdate> updates = await StoreContext.GetDefault().GetAppAndOptionalStorePackageUpdatesAsync();

                return !updates.IsEmpty();
            }
            catch
            {
                return false;
            }
        }
    }
}
