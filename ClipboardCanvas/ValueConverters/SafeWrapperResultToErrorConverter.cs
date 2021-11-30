using System;
using Windows.UI.Xaml.Data;

using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.ValueConverters
{
    public class SafeWrapperResultToErrorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not SafeWrapperResult safeWrapperResult)
            {
                return null;
            }

            return safeWrapperResult.Details?.message;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
