using ClipboardCanvas.ViewModels.UserControls;
using System;
using Windows.UI.Xaml.Data;

namespace ClipboardCanvas.ValueConverters
{
    public class InterfaceToSuggestedActionsControlViewModelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value as SuggestedActionsControlViewModel;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
