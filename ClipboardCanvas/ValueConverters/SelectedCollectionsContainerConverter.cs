using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ClipboardCanvas.ValueConverters
{
    public class SelectedCollectionsContainerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not bool boolParam)
            {
                return new Thickness(0);
            }

            if (boolParam)
            {
                return new Thickness(3);
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
