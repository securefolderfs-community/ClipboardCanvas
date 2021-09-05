using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.Extensions;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.Helpers
{
    public static class WebHelpers
    {
        public static bool IsUrl(string str)
        {
            return Uri.TryCreate(str, UriKind.Absolute, out Uri uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        public static async Task<bool> IsUrlImage(string url)
        {
            const string urlImageContentType = "image/";

            try
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp(url);
                request.Method = "HEAD";
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    if (response.StatusCode == HttpStatusCode.OK && response.ContentType.ToLowerInvariant().Contains(urlImageContentType))
                    {
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        public static async Task<bool> IsValidUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            WebResponse response = null;
            try
            {
                WebRequest request = HttpWebRequest.Create(url);
                response = await request.GetResponseAsync();
            }
            catch (WebException webEx)
            {
                if (webEx.Message.Contains("302")) // Result: "Found"
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                response?.Dispose();
            }

            return true;
        }

        public static string AlternativeGetIcon(HtmlNodeCollection linkNodes)
        {
            string iconUrl = null;

            if (!linkNodes.IsEmpty())
            {
                foreach (var tag in linkNodes)
                {
                    HtmlAttribute tagRel = tag.Attributes["rel"];
                    HtmlAttribute tagHref = tag.Attributes["href"];
                    HtmlAttribute tagSizes = tag.Attributes["sizes"];

                    if (tagRel != null && tagHref != null)
                    {
                        switch (tagRel.Value.ToLower())
                        {
                            case "shortcut icon":
                                iconUrl = string.IsNullOrEmpty(iconUrl) ? tagHref.Value : iconUrl;
                                break;
                            case "icon":
                                iconUrl = tagHref.Value;
                                break;
                        }
                    }
                }
            }

            return iconUrl;
        }

        public static SiteMetadata GetMetadata(HtmlNodeCollection metaTags)
        {
            SiteMetadata metaInfo = new SiteMetadata();

            if (!metaTags.IsEmpty())
            {
                int matchCount = 0;

                foreach (var tag in metaTags)
                {
                    HtmlAttribute tagName = tag.Attributes["name"];
                    HtmlAttribute tagContent = tag.Attributes["content"];
                    HtmlAttribute tagProperty = tag.Attributes["property"];

                    if (tagName != null && tagContent != null)
                    {
                        switch (tagName.Value.ToLower())
                        {
                            case "title":
                                metaInfo.Title = tagContent.Value;
                                matchCount++;
                                break;
                            case "description":
                                metaInfo.Description = tagContent.Value;
                                matchCount++;
                                break;
                            case "twitter:title":
                                metaInfo.Title = string.IsNullOrEmpty(metaInfo.Title) ? tagContent.Value : metaInfo.Title;
                                matchCount++;
                                break;
                            case "twitter:description":
                                metaInfo.Description = string.IsNullOrEmpty(metaInfo.Description) ? tagContent.Value : metaInfo.Description;
                                matchCount++;
                                break;
                            case "keywords":
                                metaInfo.Keywords = tagContent.Value;
                                matchCount++;
                                break;
                            case "twitter:image":
                                metaInfo.ImageUrls.AddIfNotThere(string.IsNullOrEmpty(metaInfo.IconUrl) ? tagContent.Value : metaInfo.IconUrl);
                                matchCount++;
                                break;
                            case "icon":
                                metaInfo.IconUrl = string.IsNullOrEmpty(metaInfo.IconUrl) ? tagContent.Value : metaInfo.IconUrl;
                                matchCount++;
                                break;
                        }
                    }
                    else if (tagProperty != null && tagContent != null)
                    {
                        switch (tagProperty.Value.ToLower())
                        {
                            case "og:title":
                                metaInfo.Title = string.IsNullOrEmpty(metaInfo.Title) ? tagContent.Value : metaInfo.Title;
                                matchCount++;
                                break;
                            case "og:description":
                                metaInfo.Description = string.IsNullOrEmpty(metaInfo.Description) ? tagContent.Value : metaInfo.Description;
                                matchCount++;
                                break;
                            case "og:image":
                                metaInfo.ImageUrls.AddIfNotThere(string.IsNullOrEmpty(metaInfo.IconUrl) ? tagContent.Value : metaInfo.IconUrl);
                                matchCount++;
                                break;
                            case "og:icon":
                                metaInfo.IconUrl = string.IsNullOrEmpty(metaInfo.IconUrl) ? tagContent.Value : metaInfo.IconUrl;
                                matchCount++;
                                break;
                            case "og:site":
                            case "og:site_name":
                                metaInfo.SiteName = tagContent.Value;
                                break;
                        }
                    }
                }

                metaInfo.HasAnyData = matchCount > 0;
            }

            return metaInfo;
        }

        public static async Task<List<string>> FormatImageUrls(List<string> rawImages, string url)
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

        public static async Task<string> FormatImageUrl(string rawImageUrl, string url)
        {
            if (!string.IsNullOrEmpty(rawImageUrl))
            {
                string formattedUrl;
                
                if (!rawImageUrl.Contains("http"))
                {
                    string baseUrl = null;
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
                        
                        try
                        {
                            Uri rawImageUri = new Uri(rawImageUrl);
                            if (!string.IsNullOrEmpty(rawImageUri.Host))
                            {
                                baseUrl = $"{http}{rawImageUri.Host}";
                                rawImageUrl = rawImageUrl.Replace(rawImageUri.Host, string.Empty);
                            }
                        }
                        catch { }

                        if (string.IsNullOrEmpty(baseUrl))
                        {
                            Uri uri = new Uri(url);
                            baseUrl = $"{http}{uri.Host}";
                        }
                    }
                    else
                    {
                        baseUrl = url;
                    }

                    formattedUrl = ($"{baseUrl}/{rawImageUrl}").PreventNull(string.Empty).Replace("///", "/").Replace("//", "/").Replace(":/", "://");
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

        public static async Task<bool> CheckImageUrlIfExists(string imageUrl)
        {
            SafeWrapper<bool> existsResult = await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(imageUrl);
                request.Method = "HEAD";

                request.UseDefaultCredentials = true;
                request.Accept = "*/*";
                request.Proxy.Credentials = CredentialCache.DefaultCredentials;

                using (WebResponse response = await request.GetResponseAsync())
                {
                    return true;
                }
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
    }
}
