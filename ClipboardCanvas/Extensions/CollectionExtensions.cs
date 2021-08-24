using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace ClipboardCanvas.Extensions
{
    public static class CollectionExtensions
    {
        public static int IndexFitBounds(int itemsCount, int wantedIndex) => wantedIndex < 0 ? 0 : (wantedIndex >= itemsCount ? (itemsCount == 0 ? 0 : (itemsCount - 1)) : wantedIndex);

        public static bool IsEmpty<T>(this IEnumerable<T> enumerable) => enumerable == null || !enumerable.Any();

        public static void AddFront<T>(this IList<T> list, T item) => list.Insert(0, item);

        public static void RemoveFront<T>(this IList<T> list)
        {
            if (!list.IsEmpty())
            {
                list.RemoveAt(0);
            }
        }

        public static void RemoveBack<T>(this IList<T> list)
        {
            if (!list.IsEmpty())
            {
                list.RemoveAt(list.Count - 1);
            }
        }

        public static bool CheckNotNull<T>(this IEnumerable<T> enumerable) => !(enumerable == null || enumerable.Any((item) => item.IsNull()));

        public static List<T> ToListSingle<T>(this T element) => new List<T>() { element };

        public static void DisposeClear<T>(this ICollection<T> collection)
        {
            if (collection == null)
            {
                return;
            }

            collection.DisposeAll();
            collection.Clear();
        }

        public static void DisposeAll(this IEnumerable enumerable)
        {
            if (enumerable == null)
            {
                return;
            }

            foreach (var item in enumerable)
            {
                (item as IDisposable)?.Dispose();
            }
        }
    }
}
