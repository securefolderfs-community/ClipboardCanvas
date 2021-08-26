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
using Windows.UI.Xaml.Media.Imaging;
using ClipboardCanvas.DataModels;
using System.Net.Http;
using System.Threading;

namespace ClipboardCanvas.ViewModels.UserControls.SimpleCanvasDisplay
{
    public class UrlSimpleCanvasViewModel : BaseReadOnlyCanvasViewModel
    {
        private bool _IsLoading;
        public bool IsLoading
        {
            get => _IsLoading;
            set => SetProperty(ref _IsLoading, value);
        }

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

        private string _SiteName;
        public string SiteName
        {
            get => _SiteName;
            set => SetProperty(ref _SiteName, value);
        }

        private string _Description;
        public string Description
        {
            get => _Description;
            set => SetProperty(ref _Description, value);
        }

        private BitmapImage _SiteIcon;
        public BitmapImage SiteIcon
        {
            get => _SiteIcon;
            set => SetProperty(ref _SiteIcon, value);
        }

        private bool _ContentImageLoad;
        public bool ContentImageLoad
        {
            get => _ContentImageLoad;
            set => SetProperty(ref _ContentImageLoad, value);
        }

        private BitmapImage _ContentImage1;
        public BitmapImage ContentImage1
        {
            get => _ContentImage1;
            set => SetProperty(ref _ContentImage1, value);
        }

        private BitmapImage _ContentImage2;
        public BitmapImage ContentImage2
        {
            get => _ContentImage2;
            set => SetProperty(ref _ContentImage2, value);
        }

        private BitmapImage _ContentImage3;
        public BitmapImage ContentImage3
        {
            get => _ContentImage3;
            set => SetProperty(ref _ContentImage3, value);
        }

        private BitmapImage _ContentImage4;
        public BitmapImage ContentImage4
        {
            get => _ContentImage4;
            set => SetProperty(ref _ContentImage4, value);
        }

        public static List<string> Extensions => new List<string>() {
            Constants.FileSystem.URL_CANVAS_FILE_EXTENSION
        };

        public UrlSimpleCanvasViewModel(IBaseCanvasPreviewControlView view, BaseContentTypeModel contentType)
            : base(StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, contentType, view)
        {
        }

        public override async Task<SafeWrapperResult> TryLoadExistingData(CanvasItem canvasItem, BaseContentTypeModel contentType, CancellationToken cancellationToken)
        {
            IsLoading = true;
            SafeWrapperResult result = await base.TryLoadExistingData(canvasItem, contentType, cancellationToken);
            IsLoading = false;

            return result;
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
            this.Url = url;

            SafeWrapper<string> rawHtml = await WebHelpers.GetRawHtmlPage(Url, cancellationToken);
            if (!rawHtml)
            {
                return rawHtml;
            }

            SiteMetadata metadata = GetMetadata(rawHtml);

            this._Title = metadata.Title;
            this._Description = metadata.Description;
            this._SiteName = metadata.SiteName;

            string imageLogo = await FormatImageUrl(metadata.RawIcon, url);

            if (metadata.RawImages.IsEmpty())
            {
                List<string> rawImagesFromHtml = GetRawImagesFromRawHtml(rawHtml);

                foreach (var item2 in rawImagesFromHtml)
                {
                    if (item2.ToLower().Contains("logo"))
                    {
                        imageLogo = await FormatImageUrl(item2, url);

                        break;
                    }
                    else
                    {
                        metadata.RawImages.Add(item2);
                    }
                }
            }
            List<string> imageUrls = await FormatImageUrls(metadata.RawImages, Url);

            if (!string.IsNullOrEmpty(imageLogo))
            {
                SiteIcon = new BitmapImage(new Uri(imageLogo));
            }

            if (!imageUrls.IsEmpty())
            {
                ContentImageLoad = true;

                foreach (var imageLink in imageUrls)
                {
                    if (this._ContentImage1 == null)
                    {
                        _ContentImage1 = new BitmapImage(new Uri(imageLink));
                    }
                    else if (this._ContentImage2 == null)
                    {
                        _ContentImage2 = new BitmapImage(new Uri(imageLink));
                    }
                    else if (this._ContentImage3 == null)
                    {
                        _ContentImage3 = new BitmapImage(new Uri(imageLink));
                    }
                    else if (this._ContentImage4 == null)
                    {
                        _ContentImage4 = new BitmapImage(new Uri(imageLink));
                    }
                }
            }

            return SafeWrapperResult.SUCCESS;
        }

        protected override async Task<SafeWrapperResult> TryFetchDataToView()
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(SiteName));
            OnPropertyChanged(nameof(Description));

            OnPropertyChanged(nameof(ContentImage1));
            OnPropertyChanged(nameof(ContentImage2));
            OnPropertyChanged(nameof(ContentImage3));
            OnPropertyChanged(nameof(ContentImage4));

            return SafeWrapperResult.SUCCESS;
        }

        #region Helpers

        private SiteMetadata GetMetadata(string rawHtml)
        {
            SiteMetadata metadata = new SiteMetadata();

            Regex metadataRegex = new Regex("<meta[\\s]+[^>]*?content[\\s]?=[\\s\"\']+(.*?)[\"\']+.*?>");
            MatchCollection metadataMatches = metadataRegex.Matches(rawHtml);

            if (!metadataMatches.IsEmpty())
            {
                foreach (Match item in metadataMatches)
                {
                    for (int i = 0; i <= item.Groups.Count; i++)
                    {
                        string groupValue = item.Groups[i].Value.ToString().ToLower();
                        string value = item.Groups.Next(i)?.Value;

                        if (groupValue.Contains("description"))
                        {
                            metadata.Description = value;

                            break;
                        }
                        else if (groupValue.Contains("og:site_name") || groupValue.Contains("og:site"))
                        {
                            metadata.SiteName = value;

                            break;
                        }
                        else if (groupValue.Contains("og:title"))
                        {
                            metadata.Title = value;

                            break;
                        }
                        else if (groupValue.Contains("og:image"))
                        {
                            try
                            {
                                // Sometimes, we may get numbers representing width and height
                                _ = Convert.ToInt32(value);

                                // Is a number, don't add it to list
                            }
                            catch // Not a number
                            {
                                metadata.RawImages.AddIfNotThere(value);
                            }

                            break;
                        }
                        else if (groupValue.Contains("icon"))
                        {
                            metadata.RawIcon = value;

                            break;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(metadata.Title))
            {
                metadata.Title = GetTitleFromRawHtml(rawHtml);
            }

            if (!string.IsNullOrEmpty(metadata.Description))
            {
                metadata.Description = Uri.UnescapeDataString(metadata.Description);
            }

            if (string.IsNullOrEmpty(metadata.RawIcon))
            {
                metadata.RawIcon = GetRawIcon(rawHtml);
            }

            return metadata;
        }

        private string GetRawIcon(string rawHtml)
        {
            Regex iconRegex = new Regex("<link .*? href=\"(.*?.)\"");
            MatchCollection iconMatches = iconRegex.Matches(rawHtml);

            string rawIcon = null;
            if (!iconMatches.IsEmpty())
            {
                foreach (Match item in iconMatches)
                {
                    for (int i = 0; i <= item.Groups.Count; i++)
                    {
                        string groupValue = item.Groups[i].Value.ToString().ToLower();
                        string value = item.Groups.Next(i)?.Value;

                        if (groupValue.Contains("icon") && value != null)
                        {
                            rawIcon = value;
                        }
                    }
                }
            }

            return rawIcon;
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
                return null;
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
                string formattedUrl = await FormatImageUrl(item, url);
                if (!string.IsNullOrEmpty(formattedUrl))
                {
                    validImageUrls.Add(formattedUrl);
                    if (validImageUrls.Count == Constants.UI.CanvasContent.UrlCanvas.MAX_IMAGES_TO_DISPLAY)
                    {
                        break;
                    }
                }
            }

            return validImageUrls;
        }

        private async Task<string> FormatImageUrl(string rawImageUrl, string url)
        {
            if (!string.IsNullOrEmpty(rawImageUrl))
            {
                string formattedUrl;

                if (!rawImageUrl.Contains("http"))
                {
                    string baseUrl;
                    if (rawImageUrl.EndsWith(".ico"))
                    {
                        string http;
                        if (url.Contains("https"))
                        {
                            http = "https://";
                        }
                        else
                        {
                            http = "http://";
                        }

                        var uri = new Uri(url);
                        baseUrl = $"{http}{uri.Host}";
                    }
                    else
                    {
                        baseUrl = url;
                    }

                    formattedUrl = ($"{baseUrl}/{rawImageUrl}").PreventNull(string.Empty).Replace("///", "/").Replace("//", "/").Replace("https:/", "https://");
                }
                else
                {
                    formattedUrl = rawImageUrl;
                }

                if (await CheckImageUrlIfExists(formattedUrl))
                {
                    return formattedUrl;
                }
            }

            return null;
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
