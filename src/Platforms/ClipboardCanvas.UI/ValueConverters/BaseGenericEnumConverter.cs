using System;
using System.Collections.Generic;
using System.Linq;

namespace ClipboardCanvas.UI.ValueConverters
{
    public abstract class BaseGenericEnumConverter : BaseConverter
    {
        /// <inheritdoc/>
        protected override object? TryConvert(object? value, Type targetType, object? parameter)
        {
            return ConvertInternal(value, targetType, parameter,
                s => s.Split(',').ToDictionary(k => System.Convert.ToInt64(k.Split('-')[0]), v => System.Convert.ToInt64(v.Split('-')[1])));
        }

        /// <inheritdoc/>
        protected override object? TryConvertBack(object? value, Type targetType, object? parameter)
        {
            return ConvertInternal(value, targetType, parameter,
                s => s.Split(',').ToDictionary(k => System.Convert.ToInt64(k.Split('-')[0]), v => System.Convert.ToInt64(v.Split('-')[1])));
        }

        private object ConvertInternal(object? value, Type targetType, object? parameter, Func<string, Dictionary<long, long>> enumConversion)
        {
            var numberEnumValue = Convert.ToInt64(value);
            if (parameter is string strParam)
            {
                // enumValue-convertedValue: 0-1,1-2
                var enumConversionValues = enumConversion(strParam);
                if (enumConversionValues.TryGetValue(numberEnumValue, out var convertedValue))
                    numberEnumValue = convertedValue;

                // else.. use value from the cast above
            }

            try
            {
                if (Enum.GetName(targetType, numberEnumValue) is string enumName)
                    return Enum.Parse(targetType, enumName);
            }
            catch (Exception) { }

            try
            {
                return System.Convert.ChangeType(numberEnumValue, targetType);
            }
            catch (Exception) { }

            try
            {
               return Enum.ToObject(targetType, numberEnumValue);
            }
            catch (Exception) { }

            return numberEnumValue;
        }
    }
}
