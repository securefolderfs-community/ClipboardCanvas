using ClipboardCanvas.Helpers.SafetyHelpers;
using Microsoft.Toolkit.Parsers.Markdown;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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

        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            var request = WebRequest.Create(url);
            SafeWrapper<bool> result = SafeWrapperRoutines.SafeWrap(() =>
            {
                var response = request.GetResponse() as HttpWebResponse;
                if (response.StatusCode.ToString().ToLower() == "ok")
                {
                    return true;
                }

                return false;
            });

            if (result)
            {
                return result.Result;
            }
            else
            {
                return false;
            }
        }
    }
}
