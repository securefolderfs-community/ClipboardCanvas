using ClipboardCanvas.Helpers.SafetyHelpers;
using Microsoft.Toolkit.Parsers.Markdown;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Helpers
{
    public static class WebHelpers
    {
        public static bool IsHTML(string str)
        {
            return Regex.IsMatch(str, "<(.|\n)*?>");
        }

        public static bool IsUrl(string str)
        {
            return Uri.TryCreate(str, UriKind.Absolute, out Uri uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        public static bool IsUrlFile(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return Path.HasExtension(url);
            }

            return false;
        }

        public static async Task<bool> IsValidUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            WebResponse response;
            try
            {
                WebRequest request = WebRequest.Create(url);
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
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static async Task<SafeWrapper<string>> GetRawHtmlPage(string url, CancellationToken cancellationToken)
        {
            return await SafeWrapperRoutines.SafeWrapAsync(async () =>
            {
                string rawHtml = null;
                HttpClient client = new HttpClient();
                using (HttpResponseMessage response = await client.GetAsync(new Uri(url, UriKind.Absolute), HttpCompletionOption.ResponseContentRead, cancellationToken))
                {
                    using (HttpContent content = response.Content)
                    {
                        rawHtml = await content.ReadAsStringAsync();
                    }
                }

                return rawHtml;
            });
        }
    }
}
