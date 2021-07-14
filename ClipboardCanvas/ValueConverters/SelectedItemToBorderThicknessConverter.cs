using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ClipboardCanvas.ValueConverters
{
    public class SelectedItemToBorderThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not bool boolParam)
            {
                return new Thickness(0);
            }

            if (boolParam)
            {
                return new Thickness(2);
            }
            else
            {
                return new Thickness(0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
