using System;
using Windows.UI.Xaml.Data;

namespace ClipboardCanvas.ValueConverters
{
    public sealed class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string strParam)
            {
                if (strParam.ToLower() == "invert")
                {
                    if (value is null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (value is null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
