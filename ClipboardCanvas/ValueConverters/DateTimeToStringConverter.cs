using System;
using Microsoft.UI.Xaml.Data;

namespace ClipboardCanvas.ValueConverters
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not DateTime dateTime)
            {
                return value;
            }

            if (parameter is string format)
            {
                return dateTime.ToString(format);
            }
            else
            {
                return dateTime.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
