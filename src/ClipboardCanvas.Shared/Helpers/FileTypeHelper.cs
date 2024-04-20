using ClipboardCanvas.Shared.Enums;
using System;
using System.Linq;

namespace ClipboardCanvas.Shared.Helpers
{
    public static class FileTypeHelper
    {
        public static string[] TextExtensions { get; } =
        {
            // Text based
            ".txt", ".md", ".markdown", ".rtf"
        };

        public static string[] DocumentExtensions { get; } =
        {
            // Document based
            ".doc", ".docx", ".html",
            ".odt", ".pdf", ".htm",

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

        public static TypeHint GetTypeFromMime(string mimeType)
        {
            return Image()
                   ?? PlainText()
                   ?? Document()
                   ?? Media()
                   // TODO
                   ?? TypeHint.Unclassified;

            TypeHint? Media()
            {
                return mimeType.Equals("video/x-msvideo")
                    || mimeType.Equals("video/mp4")
                    || mimeType.Equals("video/mpeg")
                    || mimeType.Equals("video/ogg")
                    || mimeType.Equals("video/webm")
                    || mimeType.Equals("video/3gpp")
                    || mimeType.Equals("video/3gpp2")

                    ? TypeHint.Media : null;
            }

            TypeHint? Document()
            {
                return mimeType.Equals("application/pdf")
                    || mimeType.Equals("text/csv")
                    || mimeType.Equals("application/msword")
                    || mimeType.Equals("application/vnd.openxmlformats-officedocument.wordprocessingml.document")

                    ? TypeHint.Document : null;
            }

            TypeHint? PlainText()
            {
                return mimeType.StartsWith("text/")
                    && !mimeType.Equals("text/csv")
                    //|| mimeType.StartsWith("")

                    ? TypeHint.PlainText : null;
            }

            TypeHint? Image()
            {
                return mimeType.StartsWith("image/")
                    //|| mimeType.StartsWith("")

                    ? TypeHint.Image : null;
            }
        }

        public static TypeHint GetTypeFromExtension(string extension)
        {
            if (!extension.StartsWith('.'))
                extension = $".{extension}";

            if (TextExtensions.Contains(extension))
                return TypeHint.PlainText;

            if (DocumentExtensions.Contains(extension))
                return TypeHint.Document;

            if (ImageExtensions.Contains(extension))
                return TypeHint.Image;

            if (MediaExtensions.Contains(extension))
                return TypeHint.Media;

            if (AudioExtensions.Contains(extension))
                return TypeHint.Audio;

            return TypeHint.Unclassified;
        }
    }
}
