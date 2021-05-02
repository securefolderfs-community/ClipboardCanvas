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

        public static async Task<BasePastedContentTypeDataModel> GetContentType(StorageFile file)
        {
            string ext = Path.GetExtension(file.Path);

            if (ImageCanvasViewModel.Extensions.Contains(ext))
            {
                return new ImageContentType();
            }

            if (TextCanvasViewModel.Extensions.Contains(ext))
            {
                return new TextContentType();
            }

            // Default, try as text
            if (await TextCanvasViewModel.CanLoadAsText(file))
            {
                return new TextContentType();
            }

            return null;
        }
    }
}
