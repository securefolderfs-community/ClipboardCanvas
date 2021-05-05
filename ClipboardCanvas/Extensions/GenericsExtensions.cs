using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.Extensions
{
    public static class GenericsExtensions
    {
        /// <summary>
        /// Converts given <paramref name="value"/> to provided <typeparamref name="TOut"/>
        /// </summary>
        /// <typeparam name="TOut">The generic type to convert to</typeparam>
        /// <typeparam name="TIn">The type to convert from</typeparam>
        /// <param name="value">The value</param>
        /// <remarks>
        /// The <typeparamref name="TOut"/> must implement <see cref="IConvertible"/>
        /// </remarks>
        /// <returns>Converted <paramref name="value"/></returns>
        public static TOut ConvertValue<TSource, TOut>(this TSource value) =>
            (TOut)Convert.ChangeType(value, typeof(TOut));

        /// <summary>
        /// Compares two generic types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value1">The first value</param>
        /// <param name="value2">The second value</param>
        /// <returns><see cref="bool"/> true if both <typeparamref name="T"/> <paramref name="value1"/> and <typeparamref name="T"/> <paramref name="value2"/> are equal</returns>
        public static bool Equals<T>(this T value1, T value2) =>
            EqualityComparer<T>.Default.Equals(value1, value2);

        /// <summary>
        /// Converts given value to a given instantiate type
        /// </summary>
        /// <typeparam name="T">The source value</typeparam>
        /// <typeparam name="TOut">The result value</typeparam>
        /// <param name="value">The first value</param>
        /// <param name="instantiate">Instantiate action</param>
        /// <returns>Converted value</returns>
        public static TOut ConvertTo<T, TOut>(this T value, Func<T, TOut> instantiate) =>
            instantiate(value);

        /// <summary>
        /// Tries to cast the given value to new type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static TOut As<T, TOut>(this T obj)
        {
            try
            {
                return (TOut)(object)obj;
            }
            catch
            {
                return default(TOut);
            }
        }
    }
}
