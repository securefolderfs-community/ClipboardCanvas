using ClipboardCanvas.Shared.Enums;
using System;
using System.Linq;

namespace ClipboardCanvas.Shared.Helpers
{
    public static class FileExtensionHelper
    {
        public static string[] DocumentExtensions { get; } =
        {
            // Text based
            ".doc", ".docx", ".html",
            ".odt", ".pdf", ".htm",
            ".txt",

            // Sheet based
            ".xls", ".xlsx", ".ods",

            // Presentation based
            ".ppt", ".pptx"
        };

        public static string[] ImageExtensions { get; } =
        {
            // Special types
            ".apng", ".avif", ".gif",

            // JPEG types
            ".jpg", ".jpeg", ".jfif",
            ".pjpeg", ".pjp",

            // Other types
            ".png", ".svg", ".webp",
            ".bmp", ".tif", ".tiff"
        };

        public static string[] MediaExtensions { get; } =
        {
            ".mp4", ".mov", ".avi",
            ".wmv", ".flv", ".webm",
            ".mkv", ".avi"
        };

        public static string[] AudioExtensions { get; } =
        {
            ".3gp", ".flac", ".mp3",
            ".ogg", ".wav"
        };

        public static FileType GetFileType(string extension)
        {
            if (!extension.StartsWith('.'))
                extension = $".{extension}";

            if (DocumentExtensions.Contains(extension))
                return FileType.Document;

            if (ImageExtensions.Contains(extension))
                return FileType.Image;

            if (MediaExtensions.Contains(extension))
                return FileType.Media;

            if (AudioExtensions.Contains(extension))
                return FileType.Audio;

            return FileType.Unclassified;
        }
    }
}
