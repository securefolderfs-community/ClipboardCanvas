using System;
using System.Collections.Generic;
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

        public static bool IsWebsiteLink(string str)
        {
            return Uri.TryCreate(str, UriKind.Absolute, out Uri uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        }
    }
}
