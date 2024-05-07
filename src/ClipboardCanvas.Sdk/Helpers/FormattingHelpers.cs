using System;
using System.Globalization;

namespace ClipboardCanvas.Sdk.Helpers
{
    public static class FormattingHelpers
    {
        public static string GetDateFileName(string extension, CultureInfo? culture = null)
        {
            // TODO: Use CultureInfo in the future to format the date string
            const string FILE_DATE_NAME_FORMAT = "HH.mm dd-MM-yyyy";

            var now = DateTime.Now;
            return $"{now.ToString(FILE_DATE_NAME_FORMAT)}{extension}";
        }
    }
}
