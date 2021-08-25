using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

using ClipboardCanvas.DataModels.ContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.ModelViews;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Extensions;

namespace ClipboardCanvas.ViewModels.UserControls.SimpleCanvasDisplay
{
    public class UrlSimpleCanvasViewModel : BaseReadOnlyCanvasViewModel
    {
        private string _Url;
        public string Url
        {
            get => _Url;
            set => SetProperty(ref _Url, value);
        }

        private string _Title;
        public string Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }

        private string _Description;
        public string Description
        {
            get => _Description;
            set => SetProperty(ref _Description, value);
        }

        public static List<string> Extensions => new List<string>() {
            Constants.FileSystem.URL_CANVAS_FILE_EXTENSION
        };

        public UrlSimpleCanvasViewModel(IBaseCanvasPreviewControlView view, BaseContentTypeModel contentType)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, contentType, view)
        {
        }

        protected override async Task<SafeWrapperResult> SetDataFromExistingItem(IStorageItem item)
        {
            if (item is not StorageFile file)
            {
                return ItemIsNotAFileResult;
            }

            SafeWrapper<string> url = await FilesystemOperations.ReadFileText(file);
            if (!url)
            {
                return url;
            }

            if (!WebHelpers.IsValidUrl(url.Result))
            {
                return new SafeWrapperResult(OperationErrorCode.InvalidArgument, new ArgumentException(), "The URL is not valid.");
            }
            this.Url = url;

            SafeWrapper<string> rawHtml = await DownloadRawHtmlPage(Url);
            if (!rawHtml)
            {
                return rawHtml;
            }

            (string title, string description, string rawImage) = GetMetadata(rawHtml);

            this.Title = title;
            this.Description = description;

            string rawImageLogo = null;
            List<string> rawImages = new List<string>();

            if (!string.IsNullOrEmpty(rawImage))
            {
                rawImages.Add(rawImage);
            }
            else
            {
                List<string> rawImagesFromHtml = GetRawImagesFromRawHtml(rawHtml);

                foreach (var item2 in rawImagesFromHtml)
                {
                    if (item2.ToLower().Contains("logo"))
                    {
                        rawImageLogo = item2;
                        rawImages.AddFront(item2);

                        break;
                    }
                    else
                    {
                        rawImages.Add(item2);
                    }
                }
            }

            List<string> imageUrls = await FormatImageUrls(rawImages, Url);


            return SafeWrapperResult.SUCCESS;
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            return SafeWrapperResult.SUCCESS;
        }

        #region Helpers

        private async Task<SafeWrapper<string>> DownloadRawHtmlPage(string url)
        {
            return await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                WebRequest request = WebRequest.Create(url);
                WebResponse response = await request.GetResponseAsync();

                string rawHtml = null;
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    rawHtml = await streamReader.ReadToEndAsync();
                }

                return rawHtml;
            });
        }

        private (string title, string description, string rawImage) GetMetadata(string rawHtml)
        {
            string title = null;
            string description = null;
            string rawImage = null;

            title = GetTitleFromRawHtml(rawHtml);

            Regex metadataRegex = new Regex("<meta[\\s]+[^>]*?content[\\s]?=[\\s\"\']+(.*?)[\"\']+.*?>");
            MatchCollection metadataMatches = metadataRegex.Matches(rawHtml);

            if (!metadataMatches.IsEmpty())
            {
                foreach (Match item in metadataMatches)
                {
                    for (int i = 0; i <= item.Groups.Count; i++)
                    {
                        string groupValue = item.Groups[i].Value.ToString().ToLower();

                        if (groupValue.Contains("description"))
                        {
                            description = item.Groups[i + 1].Value;

                            break;
                        }
                        else if (groupValue.Contains("og:title"))
                        {
                            title = item.Groups[i + 1].Value;

                            break;
                        }
                        else if (groupValue.Contains("og:image"))
                        {
                            if (string.IsNullOrEmpty(rawImage)) // Image might be already set!
                            {
                                rawImage = item.Groups[i + 1].Value;
                            }

                            break;
                        }
                        else if (groupValue.Contains("image") && groupValue.Contains("itemprop"))
                        {
                            rawImage = item.Groups[i + 1].Value;
                            if (rawImage.Length < 5)
                            {
                                rawImage = null;
                            }

                            break;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(description))
            {
                description = Uri.UnescapeDataString(description);
            }

            return (title, description, rawImage);
        }

        private string GetTitleFromRawHtml(string rawHtml)
        {
            Match match = Regex.Match(rawHtml, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return string.Empty;
            }
        }

        private List<string> GetRawImagesFromRawHtml(string rawHtml)
        {
            List<string> rawImages = new List<string>();
            Regex rawImagesRegex = new Regex(@"<img\b[^\<\>]+?\bsrc\s*=\s*[""'](?<L>.+?)[""'][^\<\>]*?\>");
            MatchCollection rawImagesMatches = rawImagesRegex.Matches(rawHtml);

            foreach (Match item in rawImagesMatches)
            {
                string imageLink = item.Groups["L"].Value;
                rawImages.Add(imageLink);
            }

            return rawImages;
        }

        private async Task<List<string>> FormatImageUrls(List<string> rawImages, string url)
        {
            List<string> validImageUrls = new List<string>();

            foreach (string item in rawImages)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    string formattedUrl;

                    if (!item.Contains("http"))
                    {
                        formattedUrl = ($"{url}/{item}").PreventNull(string.Empty).Replace("///", "/").Replace("//", "/").Replace("https:/", "https://");
                    }
                    else
                    {
                        formattedUrl = item;
                    }

                    if (await CheckImageUrlIfExists(formattedUrl))
                    {
                        validImageUrls.Add(formattedUrl);
                    }

                    if (validImageUrls.Count == Constants.UI.CanvasContent.UrlCanvas.MAX_IMAGES_TO_DISPLAY)
                    {
                        break;
                    }
                }
            }

            return validImageUrls;
        }

        private async Task<bool> CheckImageUrlIfExists(string imageUrl)
        {
            SafeWrapper<bool> existsResult = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(imageUrl);
                request.Method = "HEAD";

                request.UseDefaultCredentials = true;
                request.Accept = "*/*";
                request.Proxy.Credentials = CredentialCache.DefaultCredentials;

                await request.GetResponseAsync();

                return true;
            });

            if (existsResult)
            {
                return existsResult.Result;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
