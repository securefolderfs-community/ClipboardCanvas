using Microsoft.Toolkit.Parsers.Markdown;
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

        /// <summary>
        /// Checks whether string is a markdown. The function check whether string contains markdown format elements
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsMarkdown(string str)
        {
            MarkdownDocument document = new MarkdownDocument();
            document.Parse(str);
            
            if (document.Blocks.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
