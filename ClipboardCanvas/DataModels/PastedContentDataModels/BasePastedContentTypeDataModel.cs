using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ReferenceItems;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.DataModels.PastedContentDataModels
{
    public abstract class BasePastedContentTypeDataModel
    {
        public static async Task<BasePastedContentTypeDataModel> GetContentType(IStorageItem item, BasePastedContentTypeDataModel contentType)
        {
            if (contentType is InvalidContentTypeDataModel invalidContentType)
            {
                if (!invalidContentType.needsReinitialization)
                {
                    return invalidContentType;
                }
            }

            if (contentType != null)
            {
                return contentType;
            }

            if (item is StorageFile file)
            {
                string ext = Path.GetExtension(file.Path);

                if (ReferenceFile.IsReferenceFile(file))
                {
                    // Reference File, get the destination file extension
                    ReferenceFile referenceFile = await ReferenceFile.GetFile(file);

                    if (referenceFile.ReferencedItem == null)
                    {
                        return new InvalidContentTypeDataModel(referenceFile.LastError, false);
                    }

                    if (referenceFile.ReferencedItem is StorageFolder)
                    {
                        return new InvalidContentTypeDataModel(new SafeWrapperResult(OperationErrorCode.InvalidOperation, new InvalidOperationException(), "Displaying content for folders is not yet supported."), false);
                    }
                    else
                    {
                        file = referenceFile.ReferencedItem as StorageFile;
                    }

                    ext = Path.GetExtension(file.Path);
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
                    if (ext == Constants.FileSystem.WEBSITE_LINK_FILE_EXTENSION)
                    {
                        return new WebViewContentType(WebViewCanvasMode.ReadWebsite);
                    }

                    return new WebViewContentType(WebViewCanvasMode.ReadHtml);
                }

                // Markdown
                if (MarkdownCanvasViewModel.Extensions.Contains(ext))
                {
                    return new MarkdownContentType();
                }

                // Default, try as text
                if (await TextCanvasViewModel.CanLoadAsText(file))
                {
                    // Text
                    return new TextContentType();
                }

                // Use fallback
                return new FallbackContentType();
            }
            else if (item is StorageFolder folder)
            {
                // TODO: Handle also folders
                return new InvalidContentTypeDataModel(new SafeWrapperResult(OperationErrorCode.InvalidOperation, new InvalidOperationException(), "Displaying content for folders is not yet supported."), false);
            }

            return new InvalidContentTypeDataModel(new SafeWrapperResult(OperationErrorCode.InvalidOperation, new InvalidOperationException(), "Couldn't display content for this file"), false);
        }
    }
}
