using System;
using Windows.Storage;
using System.Diagnostics;
using System.IO;

using ClipboardCanvas.UnsafeNative;

namespace ClipboardCanvas.Services.Implementation
{
    public class ExceptionToFileLogger : ILogger
    {
        public void LogToFile(string text)
        {
            try
            {
                string filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, Constants.FileSystem.EXCEPTIONLOG_FILENAME);

                string existing = UnsafeNativeHelpers.ReadStringFromFile(filePath);
                existing += text;

                UnsafeNativeHelpers.WriteStringToFile(filePath, existing);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}
