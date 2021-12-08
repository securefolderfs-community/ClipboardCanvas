using ClipboardCanvas.ViewModels.UserControls;
using System;
using Microsoft.UI.Xaml.Data;

namespace ClipboardCanvas.ValueConverters
{
    public class InterfaceToNavigationControlViewModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value as NavigationControlViewModel;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
