using ClipboardCanvas.UI.ValueConverters;
using Microsoft.UI.Xaml.Data;
using System;

namespace ClipboardCanvas.WinUI.ValueConverters
{
    /// <inheritdoc cref="BaseNullToBoolConverter"/>
    internal sealed class NullToBoolConverter : BaseNullToBoolConverter, IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            return TryConvert(value, targetType, parameter);
        }

        /// <inheritdoc/>
        public object? ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return TryConvert(value, targetType, parameter);
        }
    }
}
