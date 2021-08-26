using System;
using System.Diagnostics;

namespace ClipboardCanvas.Extensions
{
    public static class SafetyExtensions
    {
        public static T PreventNull<T>(this T element, T defaultValue, bool throwOnNullDefaultValue = true)
        {
            if (element.IsNull()) // Is null
            {
                if (defaultValue.IsNull() && throwOnNullDefaultValue) // Default value is null
                {
                    Debugger.Break();
                    throw new NullReferenceException($"[PreventNull] Provided defautValue was null! Type: {typeof(T)}");
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return element;
            }
        }
    }
}
