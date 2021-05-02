using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.Extensions
{
    public static class LinqExtensions
    {
        /// <summary>
        /// Enumerates through <paramref name="collection"/> of elements and executes <paramref name="func"/> and returns <typeparamref name="T2"/>
        /// </summary>
        /// <typeparam name="T1">Element of <paramref name="collection"/></typeparam>
        /// <typeparam name="T2">The result of <paramref name="func"/></typeparam>
        /// <param name="collection">The collection to enumerate through</param>
        /// <param name="func">The func to take every element</param>
        /// <returns>Result of <paramref name="func"/> of every enumerated item</returns>
        public static IEnumerable<T2> ForEach<T1, T2>(this IEnumerable<T1> collection, Func<T1, T2> func)
        {
            foreach (T1 value in collection)
                yield return func(value);
        }
    }
}
