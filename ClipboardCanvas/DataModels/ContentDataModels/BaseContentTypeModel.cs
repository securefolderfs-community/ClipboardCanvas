using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ReferenceItems;
using ClipboardCanvas.Services;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.ViewModels.UserControls.SimpleCanvasDisplay;

namespace ClipboardCanvas.DataModels.ContentDataModels
{
    public abstract class BaseContentTypeModel
    {
        public static readonly SafeWrapperResult CannotDisplayContentForTypeResult = new SafeWrapperResult(OperationErrorCode.InvalidOperation, new InvalidOperationException(), "Couldn't display content for this file");

        public static readonly SafeWrapperResult CannotReceiveClipboardDataResult = new SafeWrapperResult(OperationErrorCode.AccessUnauthorized, "Couldn't retrieve clipboard data");

        public static async Task<BaseContentTypeModel> GetContentType(CanvasItem canvasFile, BaseContentTypeModel contentType)
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
            
            if ((await canvasFile.SourceItem) is StorageFolder folder)
            {
                if (FileHelpers.IsPathEqualExtension(folder.Path, Constants.FileSystem.INFINITE_CANVAS_EXTENSION))
                {
                    return new InfiniteCanvasContentType();
                }
                else
                {
                    return new FallbackContentType();
                }    
            }
            else if ((await canvasFile.SourceItem) is StorageFile file)
            {
                string ext = Path.GetExtension(file.Path);

                return await GetContentTypeFromExtension(file, ext);
            }
            else // The sourceFile was null
            {
                return new InvalidContentTypeDataModel(CannotDisplayContentForTypeResult, false);
            }
        }

        public static async Task<BaseContentTypeModel> GetContentType(IStorageItem item, BaseContentTypeModel contentType)
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
                    ReferenceFile referenceFile = await ReferenceFile.GetReferenceFile(file);

                    if (referenceFile.ReferencedItem == null)
                    {
                        return new InvalidContentTypeDataModel(referenceFile.LastError, false);
                    }

                    return await GetContentType(referenceFile.ReferencedItem, contentType);
                }

                return await GetContentTypeFromExtension(file, ext);
            }
            else if (item is StorageFolder)
            {
                IUserSettingsService userSettingsService = Ioc.Default.GetService<IUserSettingsService>();

                if (userSettingsService.AlwaysPasteFilesAsReference && !App.IsInRestrictedAccessMode)
                {
                    return new FallbackContentType();
                }
                else
                {
                    return new InvalidContentTypeDataModel(new SafeWrapperResult(OperationErrorCode.InvalidOperation | OperationErrorCode.NotAFile, new InvalidOperationException(), "Cannot paste folders without Reference Files enabled in Settings."));
                }
            }
            else
            {
                return new InvalidContentTypeDataModel(CannotDisplayContentForTypeResult, false);
            }
        }

        public static async Task<BaseContentTypeModel> GetContentTypeFromExtension(IStorageItem item, string ext)
        {
            if (item is StorageFolder folder)
            {
                if (FileHelpers.IsPathEqualExtension(folder.Path, Constants.FileSystem.INFINITE_CANVAS_EXTENSION))
                {
                    return new InfiniteCanvasContentType();
                }
                else
                {
                    return new FallbackContentType();
                }
            }

            if (item is not StorageFile file)
            {
                // Shouldn't happen
                return null;
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

            // URL preview
            if (UrlSimpleCanvasViewModel.Extensions.Contains(ext)) // TODO change this later to UrlCanvasViewModel.Extensions
            {
                return new UrlPreviewContentType();
            }

            // Default, try as text
            if (await TextCanvasViewModel.CanLoadAsText(file))
            {
                return new TextContentType();
            }

            // Use fallback
            return new FallbackContentType();
        }

        public static async Task<BaseContentTypeModel> GetContentTypeFromDataPackage(DataPackageView dataPackage)
        {
            IUserSettingsService userSettings = Ioc.Default.GetService<IUserSettingsService>();

            // Decide content type and initialize view model

            // From raw clipboard data
            if (dataPackage.Contains(StandardDataFormats.Bitmap))
            {
                // Image
                return new ImageContentType();
            }
            else if (dataPackage.Contains(StandardDataFormats.Text))
            {
                SafeWrapper<string> text = await SafeWrapperRoutines.SafeWrapAsync(() => dataPackage.GetTextAsync().AsTask());

                if (!text)
                {
                    Debugger.Break(); // What!?
                    return new InvalidContentTypeDataModel(CannotReceiveClipboardDataResult);
                }

                // Check if it's url
                if (WebHelpers.IsUrl(text))
                {
                    // The url may point to file
                    if (WebHelpers.IsUrlFile(text))
                    {
                        // Image
                        return new ImageContentType();
                    }
                    else
                    {
                        if (await WebHelpers.IsValidUrl(text))
                        {
                            // Url preview
                            return new UrlPreviewContentType();
                        }
                    }
                }

                // Fallback to text
                if (userSettings.PrioritizeMarkdownOverText)
                {
                    // Markdown
                    return new MarkdownContentType();
                }
                else
                {
                    // Normal text
                    return new TextContentType();
                }
            }
            else if (dataPackage.Contains(StandardDataFormats.StorageItems)) // From clipboard storage items
            {
                IReadOnlyList<IStorageItem> items = await dataPackage.GetStorageItemsAsync();

                if (items.Count > 1)
                {
                    // TODO: More than one item, paste in Infinite Canvas
                }
                else if (items.Count == 1)
                {
                    // One item, decide contentType for it
                    IStorageItem item = items.First();

                    BaseContentTypeModel contentType = await BaseContentTypeModel.GetContentType(item, null);

                    if (contentType is InvalidContentTypeDataModel invalidContentType && invalidContentType.error == null)
                    {
                        return new InvalidContentTypeDataModel(CannotReceiveClipboardDataResult);
                    }
                    else
                    {
                        return contentType;
                    }
                }
                else
                {
                    // No items
                    return new InvalidContentTypeDataModel(CannotReceiveClipboardDataResult);
                }
            }

            return new InvalidContentTypeDataModel(CannotReceiveClipboardDataResult);
        }
    }
}
