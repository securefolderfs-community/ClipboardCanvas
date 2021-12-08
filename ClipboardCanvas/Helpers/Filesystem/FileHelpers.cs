using System;
using System.IO;

namespace ClipboardCanvas.Helpers.Filesystem
{
    public static class FileHelpers
    {
        public static bool IsPathEqualExtension(string path, string extension)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(extension))
            {
                return false;
            }

            string pathExtension = Path.GetExtension(path);

            return pathExtension.Equals(extension, StringComparison.OrdinalIgnoreCase);
        }
    }
}
