using System.Linq;
using System.Collections.Generic;

namespace ClipboardCanvas.Extensions
{
    public static class LinqExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => enumerable == null || !enumerable.Any();

        public static bool CheckEveryNotNull<T>(this IEnumerable<T> enumerable) => !(enumerable == null || enumerable.Any((item) => item.IsNull()));

        public static List<T> ToListSingle<T>(this T element) => new List<T>() { element };

        public static T Next<T>(this IEnumerable<T> enumerable, int currentIndex) => (currentIndex + 1) >= enumerable.Count() ? default : enumerable.ElementAt(currentIndex + 1);
    }
}
