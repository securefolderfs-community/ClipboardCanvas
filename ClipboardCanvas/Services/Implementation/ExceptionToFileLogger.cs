using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Windows.Storage;

namespace ClipboardCanvas.Services.Implementation
{
    public class ExceptionToFileLogger : ILogger, IDisposable
    {
        private StorageFile _destinationFile;

        public ExceptionToFileLogger()
        {
            SetFile();
        }

        private async void SetFile()
        {
            _destinationFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("clipboardcanvas_exceptionlog.log", CreationCollisionOption.OpenIfExists);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _destinationFile != null;
        }

        public async void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            string exceptionString = "\n";

            exceptionString += $"HRESULT {exception.HResult}\n";
            exceptionString += $"MESSAGE {exception.Message}\n";
            exceptionString += $"STACKTRACE {exception.StackTrace}\n";
            exceptionString += $"SOURCE {exception.Source}\n";

            try
            {
                await FileIO.AppendTextAsync(_destinationFile, exceptionString);
            }
            catch (Exception)
            {
            }
        }

        public void Dispose()
        {
            _destinationFile = null;
        }
    }
}
