using ClipboardCanvas.WinUI.AppModels;
using Microsoft.UI.Xaml.Data;
using System;

namespace ClipboardCanvas.WinUI.ValueConverters
{
    internal sealed class MediaSourceConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            return value switch
            {
                VideoSource source => source.MediaSource,
                _ => null
            };
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
