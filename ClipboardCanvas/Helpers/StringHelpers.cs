using Microsoft.Toolkit.Parsers.Markdown;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClipboardCanvas.Helpers
{
    public static class StringHelpers
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
    }
}
