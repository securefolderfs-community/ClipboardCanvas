using System.Linq;
using System.Collections.Generic;

namespace ClipboardCanvas.Extensions
{
    public static class LinqExtensions
    {
        /// <summary>
        /// Determines whether <paramref name="enumerable"/> is empty or not.
        /// <br/><br/>
        /// Remarks:
        /// <br/>
        /// This function is faster than enumerable.Count == 0 since it'll only iterate one element instead of all elements.
        /// <br/>
        /// This function is null-safe.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => enumerable == null || !enumerable.Any();

        public static bool CheckEveryNotNull<T>(this IEnumerable<T> enumerable) => !(enumerable == null || enumerable.Any((item) => item.IsNull()));

        public static List<T> ToListSingle<T>(this T element) => new List<T>() { element };

        public static T Next<T>(this IEnumerable<T> enumerable, int currentIndex) => (currentIndex + 1) >= enumerable.Count() ? default : enumerable.ElementAt(currentIndex + 1);
    }
}
