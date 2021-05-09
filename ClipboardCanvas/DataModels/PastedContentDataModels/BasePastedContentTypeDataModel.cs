using ClipboardCanvas.ReferenceItems;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.DataModels.PastedContentDataModels
{
    public abstract class BasePastedContentTypeDataModel : IEquatable<BasePastedContentTypeDataModel>
    {
        public abstract bool Equals(BasePastedContentTypeDataModel other);

        public static async Task<BasePastedContentTypeDataModel> GetContentType(IStorageItem item)
        {
            if (item is StorageFile file)
            {
                string ext = Path.GetExtension(file.Path);

                if (ext == Constants.FileSystem.REFERENCE_FILE_EXTENSION)
                {
                    // Reference File, get the destination file extension
                    ReferenceFile referenceFile = await ReferenceFile.GetFile(file);
                    ext = Path.GetExtension(referenceFile.ReferencedFile.Path);
                }

                // Image
                if (ImageCanvasViewModel.Extensions.Contains(ext))
                {
                    return new ImageContentType();
                }

                // Text
                if (TextCanvasViewModel.Extensions.Contains(ext))
                {
                    return new TextContentType();
                }

                // Media
                if (MediaCanvasViewModel.Extensions.Contains(ext))
                {
                    return new MediaContentType();
                }

                // WebView
                if (WebViewCanvasViewModel.Extensions.Contains(ext))
                {
                    return new WebViewContentType();
                }

                // Default, try as text
                if (await TextCanvasViewModel.CanLoadAsText(file))
                {
                    // Text
                    return new TextContentType();
                }
                return null;
            }
            else if (item is StorageFolder folder)
            {
                // TODO: Handle also folders?
                return null;
            }

            return null;
        }
    }
}
