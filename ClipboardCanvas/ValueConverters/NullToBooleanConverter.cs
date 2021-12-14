using System;
using Microsoft.UI.Xaml.Data;

namespace ClipboardCanvas.ValueConverters
{
    public sealed class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string strParam && strParam.ToLower() == "invert")
            {
                return value is null;
            }

            return value is not null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
