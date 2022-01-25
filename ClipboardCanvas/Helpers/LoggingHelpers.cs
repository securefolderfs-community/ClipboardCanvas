using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClipboardCanvas.Services;
using ClipboardCanvas.Services.Implementation;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace ClipboardCanvas.Helpers
{
    public static class LoggingHelpers
    {
        public static void SafeLogExceptionToFile(Exception ex)
        {
            if (ex == null)
            {
                return;
            }

            string exceptionString = "";

            exceptionString += DateTime.Now.ToString(Constants.FileSystem.EXCEPTION_BLOCK_DATE_FORMAT);
            exceptionString += "\n";
            exceptionString += $">>> HRESULT {ex.HResult}\n";
            exceptionString += $">>> MESSAGE {ex.Message}\n";
            exceptionString += $">>> STACKTRACE {ex.StackTrace}\n";
            exceptionString += $">>> INNER {ex.InnerException}\n";
            exceptionString += $">>> SOURCE {ex.Source}\n\n";

            ILogger logger;
            try
            {
                logger = Ioc.Default.GetService<ILogger>(); // Try get Ioc logger
            }
            catch
            {
                logger = new ExceptionToFileLogger(); // Use default logger
            }

            logger?.LogToFile(exceptionString);
        }
    }
}
