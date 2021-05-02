using ClipboardCanvas.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.Helpers
{
    public static class FileSelectionHelpers
    {
        public static (IStorageFile file, PastedContentFileType fileType) GetContentCandidateFile(List<IStorageFile> files)
        {
            // TODO: Proper selection
            List<IStorageFile> newFiles = files.Where((item) =>
            {
                PastedContentFileType fileType = GetFileType(item);
                return fileType != PastedContentFileType.Unknown;
            }).ToList();

            IStorageFile candidate = newFiles.FirstOrDefault();

            if (candidate == null)
            {
                return (null, PastedContentFileType.Unknown);
            }

            return (candidate, GetFileType(candidate));
        }

        public static PastedContentFileType GetFileType(IStorageFile file)
        {
            string extension = Path.GetExtension(file.Path);

            if (
                extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
                )
            {
                return PastedContentFileType.ImageFile;
            }
            else if (
                extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase)
                )
            {
                return PastedContentFileType.VideoFile;
            }
            else if (
                extension.Equals(".txt", StringComparison.OrdinalIgnoreCase)
                )
            {
                return PastedContentFileType.TextFile;
            }

            return PastedContentFileType.Unknown;
        }
    }
}
