using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.Logging
{
    public class ExceptionLogger : ILogger
    {
        private StorageFile _destinationFile;

        public ExceptionLogger()
        {
            GetFile();
        }

        public async void GetFile()
        {
            _destinationFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("clipboardcanvas_exceptionlog.log", CreationCollisionOption.OpenIfExists);
        }

        public async void Log(string message)
        {
            if (_destinationFile == null || string.IsNullOrEmpty(message))
            {
                return;
            }

            DateTime currentTime = DateTime.Now;

            string compiledString = $"[{currentTime.ToString()}] {message}";

            try
            {
                await FileIO.AppendTextAsync(_destinationFile, compiledString);
            }
            catch (Exception)
            {
            }
        }
    }
}
