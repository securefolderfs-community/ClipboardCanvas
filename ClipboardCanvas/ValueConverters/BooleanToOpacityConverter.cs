using System;
using Windows.UI.Xaml.Data;

namespace ClipboardCanvas.ValueConverters
{
    public class BooleanToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not bool boolParam)
            {
                return 0.0d;
            }

            if (boolParam)
            {
                return 1.0d;
            }
            else
            {
                return 0.0d;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
