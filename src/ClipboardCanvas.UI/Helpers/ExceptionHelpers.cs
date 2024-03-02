using System;

namespace ClipboardCanvas.UI.Helpers
{
    public static class ExceptionHelpers
    {
        public static string? FormatException(Exception? ex)
        {
            if (ex is null)
                return null;

            var exceptionString = string.Empty;

            exceptionString += DateTime.Now.ToString(Constants.Application.EXCEPTION_BLOCK_DATE_FORMAT);
            exceptionString += "\n";
            exceptionString += $">>> HRESULT {ex.HResult}\n";
            exceptionString += $">>> MESSAGE {ex.Message}\n";
            exceptionString += $">>> STACKTRACE {ex.StackTrace}\n";
            exceptionString += $">>> INNER {ex.InnerException}\n";
            exceptionString += $">>> SOURCE {ex.Source}\n\n";

            return exceptionString;
        }
    }
}
