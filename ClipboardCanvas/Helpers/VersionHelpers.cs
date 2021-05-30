using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.Helpers
{
    public static class VersionHelpers
    {
        public static bool IsVersionDifferentThan(string versionToCompareWith, string otherVersion)
        {
            Version version1 = new Version(versionToCompareWith);
            Version version2 = new Version(otherVersion);

            return version1 != version2;
        }

        public static bool IsVersionBiggerThan(string version1, string version2)
        {
            Version vVersion1 = new Version(version1);
            Version vVersion2 = new Version(version2);

            return vVersion1 > vVersion2;
        }

        public static bool IsVersionSmallerThan(string version1, string version2)
        {
            Version vVersion1 = new Version(version1);
            Version vVersion2 = new Version(version2);

            return vVersion1 < vVersion2;
        }
    }
}
