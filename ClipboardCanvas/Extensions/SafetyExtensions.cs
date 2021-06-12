using System.Collections.Generic;

namespace ClipboardCanvas.Extensions
{
    public static class SafetyExtensions
    {
        public static T PreventNull<T>(this T element, T defaultValue)
        {
            if (EqualityComparer<T>.Default.Equals(element, default)) // Is null
            {
                return defaultValue;
            }
            else
            {
                return element;
            }
        }
    }
}
