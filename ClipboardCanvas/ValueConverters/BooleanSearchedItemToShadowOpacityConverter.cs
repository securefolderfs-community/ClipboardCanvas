using System;
using Windows.UI.Xaml.Data;

namespace ClipboardCanvas.ValueConverters
{
    public class BooleanSearchedItemToShadowOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not bool boolValue)
            {
                return 0.0d;
            }

            return boolValue ? 0.9d : 0.0d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
