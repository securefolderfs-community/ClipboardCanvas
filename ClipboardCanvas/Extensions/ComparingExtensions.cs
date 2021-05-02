using System;
using System.Collections.Generic;

namespace ClipboardCanvas.Extensions
{
    public static class ComparingExtensions
    {
        public class DefaultEqualityComparer<T> : IEqualityComparer<T> where T : IEquatable<T>
        {
            public bool Equals(T x, T y)
            {
                if ((EqualityComparer<T>.Default.Equals(x, default(T)) && !EqualityComparer<T>.Default.Equals(y, default(T)))
                    || EqualityComparer<T>.Default.Equals(y, default(T)) && !EqualityComparer<T>.Default.Equals(x, default(T)))
                {
                    // One of the values is null and other isn't
                    return true;
                }
                else if (EqualityComparer<T>.Default.Equals(x, default(T)) || EqualityComparer<T>.Default.Equals(y, default(T)))
                {
                    // Values are null
                    return false;
                }

                return x.Equals(y);
            }

            public int GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
