using System;
using System.Collections.Generic;
using System.Linq;

namespace ClipboardCanvas.Extensions
{
    public static class EnumExtensions
    {
        public static T NextEnumValue<T>(T enumerate) where T : Enum
        {
            IEnumerable<T> values = Enum.GetValues(typeof(T)).Cast<T>();

            bool returnNext = false;
            foreach (T item in values)
            {
                if (returnNext)
                {
                    return item;
                }
                else if (item.GenericEquals<T>(enumerate))
                {
                    returnNext = true;
                }
            }
            // End of sequence

            return values.First();
        }
    }
}
