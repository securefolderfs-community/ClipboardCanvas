using System;
using Windows.UI.Xaml.Data;

namespace ClipboardCanvas.ValueConverters
{
    public class ImplicitDataTemplateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return null;
            }

            string key = value.GetType().Name;
            if (App.Current.Resources.TryGetValue(key, out object dataTemplate))
            {
                return dataTemplate;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
