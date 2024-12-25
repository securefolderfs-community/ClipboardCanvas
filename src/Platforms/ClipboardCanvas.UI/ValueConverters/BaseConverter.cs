using System;

namespace ClipboardCanvas.UI.ValueConverters
{
    /// <summary>
    /// Base class for value converters.
    /// </summary>
    public abstract class BaseConverter
    {
        protected abstract object? TryConvert(object? value, Type targetType, object? parameter);

        protected abstract object? TryConvertBack(object? value, Type targetType, object? parameter);
    }
}
