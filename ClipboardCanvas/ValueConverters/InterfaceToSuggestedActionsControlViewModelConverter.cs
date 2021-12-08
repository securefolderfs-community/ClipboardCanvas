using System;
using Microsoft.UI.Xaml.Data;
using ClipboardCanvas.ViewModels.UserControls;

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
