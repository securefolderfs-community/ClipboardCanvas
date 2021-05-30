using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;

namespace ClipboardCanvas.Helpers
{
    public static class ApplicationHelpers
    {
        public static async Task<(BitmapImage icon, string appName)> GetIconFromFileHandlingApp(StorageFile file, string fileExtension)
        {
            IReadOnlyList<AppInfo> apps = await Launcher.FindFileHandlersAsync(fileExtension);

            AppInfo app = apps.FirstOrDefault();

            if (app == null)
            {
                return (null, null);
            }

            RandomAccessStreamReference stream = app.DisplayInfo.GetLogo(new Size(256, 256));

            BitmapImage bitmap = await ImagingHelpers.ToBitmapAsync((await stream.OpenReadAsync()).AsStreamForRead());

            return (bitmap, app.DisplayInfo.DisplayName);
        }

        public static bool IsVersionDifferentThan(string versionToCompareWith, string otherVersion)
        {
            Version version1 = new Version(versionToCompareWith);
            Version version2 = new Version(otherVersion);

            int numResult = version1.CompareTo(version2);

            return numResult != 0;
        }

        public static bool IsVersionBiggerThan(string version1, string version2)
        {
            version1 = version1.Replace("v", string.Empty);
            version2 = version2.Replace("v", string.Empty);

            try
            {
                Version vVersion1 = new Version(version1);
                Version vVersion2 = new Version(version2);

                return vVersion1 > vVersion2;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsVersionSmallerThan(string version1, string version2)
        {
            Version vVersion1 = new Version(version1);
            Version vVersion2 = new Version(version2);

            return vVersion1 < vVersion2;
        }
    }
}
